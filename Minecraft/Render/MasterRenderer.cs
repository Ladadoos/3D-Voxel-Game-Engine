using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System.Threading;

namespace Minecraft
{
    class ChunkBufferLayout
    {
        public float[] positions;
        public float[] textureCoordinates;
        public float[] lights;
        public float[] normals;
        public int indicesCount;
    }

    class MasterRenderer
    {
        struct ChunkRemeshLayout
        {
            public RenderChunk renderChunk;
            public ChunkBufferLayout chunkLayout;
        }

        private const float colorClearR = 0.57F;
        private const float colorClearG = 0.73F;
        private const float colorClearB = 1.0F;

        private readonly ShaderBasic basicShader;
        private readonly EntityShader entityShader;
        private readonly CameraController cameraController;
        private readonly WireframeRenderer wireframeRenderer;
        private readonly DebugHelper debugHelper;
        private readonly PlayerHoverBlockRenderer playerBlockRenderer;
        private readonly TextureAtlas textureAtlas;
        private readonly BlockModelRegistry blockModelRegistry;
        private readonly EntityMeshRegistry entityMeshRegistry;
        private readonly ScreenQuad screenQuad;
        private readonly UIRenderer uiRenderer;
        private readonly UICanvasIngame ingameCanvas;

        private readonly Dictionary<Vector2, RenderChunk> toRenderChunks = new Dictionary<Vector2, RenderChunk>();
        private readonly HashSet<Chunk> toRemeshChunks = new HashSet<Chunk>();
        private readonly OpaqueMeshGenerator blocksMeshGenerator;

        private readonly Queue<ChunkRemeshLayout> availableChunkMeshes = new Queue<ChunkRemeshLayout>();
        private readonly object meshLock = new object();
        private readonly Thread meshGenerationThread;

        private readonly Game game;

        public MasterRenderer(Game game)
        {
            this.game = game;
            basicShader = new ShaderBasic();
            entityShader = new EntityShader();
            cameraController = new CameraController(game.window);

            SetActiveCamera(game.player.camera);

            int textureAtlasId = TextureLoader.LoadTexture("../../Resources/texturePack.png");
            textureAtlas = new TextureAtlas(textureAtlasId, 256, 16);
            blockModelRegistry = new BlockModelRegistry(textureAtlas);
            blocksMeshGenerator = new OpaqueMeshGenerator(blockModelRegistry);
            entityMeshRegistry = new EntityMeshRegistry(textureAtlas);
            screenQuad = new ScreenQuad(game.window);
            wireframeRenderer = new WireframeRenderer(this);
            if(wireframeRenderer == null)
                System.Console.WriteLine("ok");
            if(game == null)
                System.Console.WriteLine("ok1");
            debugHelper = new DebugHelper(game, wireframeRenderer);
            playerBlockRenderer = new PlayerHoverBlockRenderer(wireframeRenderer, game.player);

            uiRenderer = new UIRenderer(game.window, cameraController);
            ingameCanvas = new UICanvasIngame(game);
            AddCanvas(ingameCanvas);

            EnableDepthTest();
            EnableCulling();

            meshGenerationThread = new Thread(MeshGeneratorThread);
            meshGenerationThread.IsBackground = true;
            meshGenerationThread.Start();
        }

        public void SetActiveCamera(Camera camera)
        {
            if (cameraController.camera != null)
            {
                cameraController.camera.OnProjectionChangedHandler -= OnPlayerCameraProjectionChanged;
            }
            camera.OnProjectionChangedHandler += OnPlayerCameraProjectionChanged;
            cameraController.ControlCamera(camera);
            UploadActiveCameraProjectionMatrix();
        }

        public Camera GetActiveCamera() => cameraController.camera;
        public void AddCanvas(UICanvas canvas) => uiRenderer.AddCanvas(canvas);
        public void RemoveCanvas(UICanvas canvas) => uiRenderer.RemoveCanvas(canvas);
        public Font GetFont(FontType type) => uiRenderer.GetFont(type);

        public void Render(World world)
        {
            GL.Enable(EnableCap.DepthTest);
            screenQuad.Bind();
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            basicShader.Start();
            basicShader.LoadTexture(basicShader.location_TextureAtlas, 0, textureAtlas.textureId);
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);

            lock (meshLock)
            {
                foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
                {
                    if (chunkToRender.Value.hardBlocksModel == null) continue;

                    Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                    Vector3 max = min + new Vector3(16, 256, 16);
                    if (!cameraController.camera.viewFrustum.IsAABBInFrustum(new AxisAlignedBox(min, max)))
                    {
                        continue;
                    }
                    chunkToRender.Value.hardBlocksModel.BindVAO();
                    basicShader.LoadMatrix(basicShader.location_TransformationMatrix, chunkToRender.Value.transformationMatrix);
                    GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.hardBlocksModel.indicesCount);
                }
            }

            entityShader.Start();
            entityShader.LoadTexture(entityShader.location_TextureAtlas, 0, textureAtlas.textureId);
            entityShader.LoadMatrix(entityShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);
            foreach (Entity entity in world.loadedEntities.Values)
            {
                if (entityMeshRegistry.models.TryGetValue(entity.entityType, out VAOModel entityMeshModel))
                {
                    entityMeshModel.BindVAO();
                    entityShader.LoadMatrix(entityShader.location_TransformationMatrix, Matrix4.Identity * Matrix4.CreateTranslation(entity.position));
                    GL.DrawArrays(PrimitiveType.Quads, 0, entityMeshModel.indicesCount);
                }
            }

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            playerBlockRenderer.RenderSelection();
            debugHelper.UpdateAndRender();        
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            uiRenderer.Render();
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);

            screenQuad.Unbind();
            screenQuad.RenderToScreen();
        }
        
        public void RenderChunkBorders()
        {
            lock (meshLock)
            {
                foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
                {
                    if (chunkToRender.Value.hardBlocksModel == null) continue;

                    Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                    wireframeRenderer.RenderWireframeAt(1, min, new Vector3(16, 256, 16));
                }
            }
        }

        private void MeshGeneratorThread()
        {
            while (true)
            {
                Thread.Sleep(5);

                lock (meshLock)
                {
                    Chunk remeshedChunk = null;
                    foreach (Chunk chunk in toRemeshChunks)
                    {
                        Vector2 gridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                        if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                        {
                            renderChunk = new RenderChunk(chunk.gridX, chunk.gridZ);
                            toRenderChunks.Add(gridPosition, renderChunk);
                        }

                        availableChunkMeshes.Enqueue(new ChunkRemeshLayout()
                        {
                            renderChunk = renderChunk,
                            chunkLayout = blocksMeshGenerator.GenerateMeshFor(game.world, chunk)
                        });

                        remeshedChunk = chunk;
                        break;
                    }

                    if(remeshedChunk != null)
                    {
                        toRemeshChunks.Remove(remeshedChunk);
                    }
                }
            }
        }

        public void EndFrameUpdate(World world)
        {
            RemeshChunkIfMeshAvailable();
            cameraController.Update();
        }

        private void RemeshChunkIfMeshAvailable()
        {
            bool foundChunkToRemesh = false;
            ChunkRemeshLayout chunkMesh = new ChunkRemeshLayout();

            lock(meshLock)
            {
                if(availableChunkMeshes.Count > 0)
                {
                    chunkMesh = availableChunkMeshes.Dequeue();
                    foundChunkToRemesh = true;
                }
            }

            if(foundChunkToRemesh)
            {
                chunkMesh.renderChunk.hardBlocksModel?.CleanUp();
                chunkMesh.renderChunk.hardBlocksModel = new VAOModel(chunkMesh.chunkLayout);
            }
        }

        public void OnChunkLoaded(World world, Chunk chunk)
        {
            MeshChunk(chunk);
            MeshNeighbourChunks(world, chunk);
        }

        private void MeshNeighbourChunks(World world, Chunk chunk, bool cXNegPredicate = true, bool cXPosPredicate = true, 
            bool cZNegPredicate = true, bool cZPosPredicate = true)
        {
            if (cXNegPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg))
            {
                MeshChunk(cXNeg);
            }
            if (cXPosPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos))
            {
                MeshChunk(cXPos);
            }
            if (cZNegPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg))
            {
                MeshChunk(cZNeg);
            }
            if (cZPosPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos))
            {
                MeshChunk(cZPos);
            }
        }

        public void OnChunkUnloaded(World world, Chunk chunk)
        {
            lock (meshLock)
            {
                toRenderChunks.Remove(new Vector2(chunk.gridX, chunk.gridZ));
                toRemeshChunks.Remove(chunk);
            }
            MeshNeighbourChunks(world, chunk);
        }

        public void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            MeshChunkAndSurroundings(world, chunk, blockPos, newState);
        }

        public void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, int chainPos, int chainCount)
        {
            if(chainPos == chainCount)
            {
                MeshChunkAndSurroundings(world, chunk, blockPos, oldState);
            }
        }

        private void MeshChunk(Chunk chunk)
        {
            lock (meshLock)
            {
                if (!toRemeshChunks.Contains(chunk))
                {
                    toRemeshChunks.Add(chunk);
                }
            }
        }

        private void MeshChunkAndSurroundings(World world, Chunk chunk, Vector3i blockPos, BlockState state)
        {
            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            MeshChunk(chunk);
            MeshNeighbourChunks(world, chunk, localX == 0, localX == 15, localZ == 0, localZ == 15);
        }

        private void OnPlayerCameraProjectionChanged(ProjectionMatrixInfo pInfo)
        {
            screenQuad.AdjustToWindowSize(pInfo.windowWidth, pInfo.windowHeight);
            UploadActiveCameraProjectionMatrix();
        }

        private void UploadActiveCameraProjectionMatrix()
        {
            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, GetActiveCamera().currentProjectionMatrix);
            entityShader.Start();
            entityShader.LoadMatrix(entityShader.location_ProjectionMatrix, GetActiveCamera().currentProjectionMatrix);
            entityShader.Stop();
        }

        public void CleanUp()
        {
            basicShader.CleanUp();
            TextureLoader.CleanUp();
            wireframeRenderer.CleanUp();

            lock(meshLock)
            {
                foreach(KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
                {
                    chunkToRender.Value.CleanUp();
                }
            }
        }

        private void EnableCulling()
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        private void DisableCulling()
        {
            GL.Disable(EnableCap.CullFace);
        }

        /// <summary> Renders only the lines formed by connecting the vertices together.
        private void EnableLineModeRendering()
        {
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        }

        /// <summary> Enabling depth test insures that object A behind object B isn't rendered over object B </summary>
        private void EnableDepthTest()
        {
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
