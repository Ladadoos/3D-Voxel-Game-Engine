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
        public readonly DebugHelper DebugHelper;
        private readonly PlayerHoverBlockRenderer playerBlockRenderer;
        private readonly TextureAtlas textureAtlas;
        private readonly BlockModelRegistry blockModelRegistry;
        private readonly EntityMeshRegistry entityMeshRegistry;
        private readonly ScreenQuad screenQuad;
        private readonly UIRenderer uiRenderer;
        public  readonly UICanvasIngame IngameCanvas;

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
            cameraController = new CameraController(game.Window);

            SetActiveCamera(game.ClientPlayer.camera);

            int textureAtlasId = TextureLoader.LoadTexture("../../Resources/texturePack.png");
            textureAtlas = new TextureAtlas(textureAtlasId, 256, 16);
            blockModelRegistry = new BlockModelRegistry(textureAtlas);
            blocksMeshGenerator = new OpaqueMeshGenerator(blockModelRegistry);
            entityMeshRegistry = new EntityMeshRegistry(textureAtlas);
            screenQuad = new ScreenQuad(game.Window);
            wireframeRenderer = new WireframeRenderer(this);
            DebugHelper = new DebugHelper(game, wireframeRenderer);
            playerBlockRenderer = new PlayerHoverBlockRenderer(wireframeRenderer, game.ClientPlayer);

            uiRenderer = new UIRenderer(game.Window, cameraController);
            IngameCanvas = new UICanvasIngame(game);
            AddCanvas(IngameCanvas);

            EnableDepthTest();
            EnableCulling();

            meshGenerationThread = new Thread(MeshGeneratorThread);
            meshGenerationThread.IsBackground = true;
            meshGenerationThread.Start();
        }

        public void SetActiveCamera(Camera camera)
        {
            if (cameraController.Camera != null)
            {
                cameraController.Camera.OnProjectionChangedHandler -= OnPlayerCameraProjectionChanged;
            }
            camera.OnProjectionChangedHandler += OnPlayerCameraProjectionChanged;
            cameraController.ControlCamera(camera);
            UploadActiveCameraProjectionMatrix();
        }

        public Camera GetActiveCamera() => cameraController.Camera;
        public void AddCanvas(UICanvas canvas) => uiRenderer.AddCanvas(canvas);
        public void RemoveCanvas(UICanvas canvas) => uiRenderer.RemoveCanvas(canvas);

        public void Render(World world)
        {
            GL.Enable(EnableCap.DepthTest);
            screenQuad.Bind();
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            basicShader.Start();
            basicShader.LoadTexture(basicShader.Location_TextureAtlas, 0, textureAtlas.ID);
            basicShader.LoadMatrix(basicShader.Location_ViewMatrix, cameraController.Camera.CurrentViewMatrix);

            lock (meshLock)
            {
                foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
                {
                    if (chunkToRender.Value.HardBlocksModel == null) continue;

                    Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                    Vector3 max = min + new Vector3(16, 256, 16);
                    if (!cameraController.Camera.IsAABBInViewFrustum(new AxisAlignedBox(min, max)))
                    {
                        continue;
                    }
                    chunkToRender.Value.HardBlocksModel.BindVAO();
                    basicShader.LoadMatrix(basicShader.Location_TransformationMatrix, chunkToRender.Value.TransformationMatrix);
                    GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.HardBlocksModel.IndicesCount);
                }
            }

            entityShader.Start();
            entityShader.LoadTexture(entityShader.Location_TextureAtlas, 0, textureAtlas.ID);
            entityShader.LoadMatrix(entityShader.Location_ViewMatrix, cameraController.Camera.CurrentViewMatrix);
            foreach (Entity entity in world.loadedEntities.Values)
            {
                if (entityMeshRegistry.Models.TryGetValue(entity.EntityType, out VAOModel entityMeshModel))
                {
                    entityMeshModel.BindVAO();
                    entityShader.LoadMatrix(entityShader.Location_TransformationMatrix, Matrix4.Identity * Matrix4.CreateTranslation(entity.Position));
                    GL.DrawArrays(PrimitiveType.Quads, 0, entityMeshModel.IndicesCount);
                }
            }

            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            playerBlockRenderer.RenderSelection();
            DebugHelper.UpdateAndRender();        
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
                    if (chunkToRender.Value.HardBlocksModel == null) continue;

                    Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                    wireframeRenderer.RenderWireframeAt(1, min, new Vector3(16, 256, 16), new Vector3(0, 0, 1));
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
                        Vector2 gridPosition = new Vector2(chunk.GridX, chunk.GridZ);

                        if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                        {
                            renderChunk = new RenderChunk(chunk.GridX, chunk.GridZ);
                            toRenderChunks.Add(gridPosition, renderChunk);
                        }

                        availableChunkMeshes.Enqueue(new ChunkRemeshLayout()
                        {
                            renderChunk = renderChunk,
                            chunkLayout = blocksMeshGenerator.GenerateMeshFor(game.World, chunk)
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
                chunkMesh.renderChunk.HardBlocksModel?.CleanUp();
                chunkMesh.renderChunk.HardBlocksModel = new VAOModel(chunkMesh.chunkLayout);
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
                world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg))
            {
                MeshChunk(cXNeg);
            }
            if (cXPosPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos))
            {
                MeshChunk(cXPos);
            }
            if (cZNegPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg))
            {
                MeshChunk(cZNeg);
            }
            if (cZPosPredicate && 
                world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos))
            {
                MeshChunk(cZPos);
            }
        }

        public void OnChunkUnloaded(World world, Chunk chunk)
        {
            lock (meshLock)
            {
                toRenderChunks.Remove(new Vector2(chunk.GridX, chunk.GridZ));
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
            MeshNeighbourChunks(world, chunk, localX == 0, localX == 15, localZ == 0, localZ == 15);
            MeshChunk(chunk);
        }

        private void OnPlayerCameraProjectionChanged(ProjectionMatrixInfo pInfo)
        {
            screenQuad.AdjustToWindowSize(pInfo.WindowPixelWidth, pInfo.WindowPixelHeight);
            UploadActiveCameraProjectionMatrix();
        }

        private void UploadActiveCameraProjectionMatrix()
        {
            basicShader.Start();
            basicShader.LoadMatrix(basicShader.Location_ProjectionMatrix, GetActiveCamera().CurrentProjectionMatrix);
            entityShader.Start();
            entityShader.LoadMatrix(entityShader.Location_ProjectionMatrix, GetActiveCamera().CurrentProjectionMatrix);
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
