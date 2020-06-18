using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System.Threading;
using System;

namespace Minecraft
{
    class ChunkBufferLayout
    {
        public float[] positions;
        public float[] textureCoordinates;
        public uint[] lights;
        public float[] normals;
        public int indicesCount;
    }

    class MasterRenderer
    {
        struct ChunkRemeshLayout
        {
            public Vector2 chunkGridPosition;
            public ChunkBufferLayout chunkLayout;
        }

        //The colors used to clear the framebuffer with
        private const float colorClearR = 0.02F;
        private const float colorClearG = 0.01F;
        private const float colorClearB = 0.03F;

        private readonly Game game;

        public  readonly DebugHelper DebugHelper;
        public  readonly UICanvasIngame IngameCanvas;
        public  readonly int DitherTextureId;
        private readonly ShaderBasic basicShader;
        private readonly EntityShader entityShader;
        private readonly CameraController cameraController;
        private readonly WireframeRenderer wireframeRenderer;
        private readonly PlayerHoverBlockRenderer playerBlockRenderer;
        private readonly TextureAtlas textureAtlas;
        private readonly BlockModelRegistry blockModelRegistry;
        private readonly EntityMeshRegistry entityMeshRegistry;
        private readonly ScreenQuad screenQuad;
        private readonly UIRenderer uiRenderer;
        private readonly Skydome skydome;
        private readonly OpaqueMeshGenerator blocksMeshGenerator;

        //The chunks that are currently being rendered
        private readonly Dictionary<Vector2, RenderChunk> toRenderChunks = new Dictionary<Vector2, RenderChunk>();

        //The chunks that are awaiting to be remeshed
        private readonly LinkedList<Chunk> toRemeshChunksQueue = new LinkedList<Chunk>();
        private readonly HashSet<Chunk> toRemeshChunksSet = new HashSet<Chunk>();

        //The meshes of chunks that have been generated and are awaiting to be handled and added to the rendering chunks
        private readonly List<ChunkRemeshLayout> availableChunkMeshes = new List<ChunkRemeshLayout>();

        private readonly object meshLock = new object();
        private readonly Thread meshGenerationThread;

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
            DitherTextureId = TextureLoader.LoadDitherTexture();
            skydome = new Skydome(game);

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

            skydome.Render();

            basicShader.Start();
            basicShader.LoadTexture(basicShader.Location_TextureAtlas, 0, textureAtlas.ID);
            basicShader.LoadMatrix(basicShader.Location_ViewMatrix, cameraController.Camera.CurrentViewMatrix);
            basicShader.LoadVector(basicShader.Location_SunColor, world.Environment.GetCurrentSunColor());
            basicShader.LoadVector(basicShader.Location_AmbientColor, world.Environment.AmbientColor);

            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                if (chunkToRender.Value.HardBlocksModel == null)
                    throw new Exception();

                Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                Vector3 max = min + new Vector3(16, 256, 16);
                if (!cameraController.Camera.IsAABBInViewFrustum(new AxisAlignedBox(min, max)))
                {
                    continue;
                }
                chunkToRender.Value.HardBlocksModel.BindVAO();
                basicShader.LoadMatrix(basicShader.Location_TransformationMatrix, chunkToRender.Value.TransformationMatrix);
                GL.DrawArrays(PrimitiveType.Triangles, 0, chunkToRender.Value.HardBlocksModel.IndicesCount);
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
            foreach (KeyValuePair<Vector2, Chunk> chunkToRender in game.World.loadedChunks)
            {
                Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                wireframeRenderer.RenderWireframeAt(1, min, new Vector3(16, 256, 16), new Vector3(0, 0, 1));
            }

            foreach(KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                 if(chunkToRender.Value.HardBlocksModel == null)
                   throw new Exception();

                Vector3 min = new Vector3(chunkToRender.Key.X * 16 + 4, 0, chunkToRender.Key.Y * 16 + 4);
                wireframeRenderer.RenderWireframeAt(1, min, new Vector3(8, 256, 8), new Vector3(1, 0, 1));
            }
        }

        private void MeshGeneratorThread()
        {
            while (true)
            {
                Thread.Sleep(5);

                lock (meshLock)
                {
                    if(toRemeshChunksQueue.Count <= 0)
                        continue;

                    Chunk chunk = toRemeshChunksQueue.First.Value;
                    toRemeshChunksQueue.RemoveFirst();
                    if(!toRemeshChunksSet.Remove(chunk))
                        throw new Exception();

                    //Generate the mesh for the chunk and add it to the queue to be used later
                    availableChunkMeshes.Add(new ChunkRemeshLayout()
                    {
                        chunkGridPosition = new Vector2(chunk.GridX, chunk.GridZ),
                        chunkLayout = blocksMeshGenerator.GenerateMeshFor(game.World, chunk)
                    });
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
            const int meshesPerFrame = 2;
            for(int i = 0; i < meshesPerFrame; i++)
            {
                bool foundChunkToRemesh = false;
                ChunkRemeshLayout chunkMesh = new ChunkRemeshLayout();

                lock(meshLock)
                {
                    if(availableChunkMeshes.Count > 0)
                    {
                        chunkMesh = availableChunkMeshes[0];
                        availableChunkMeshes.RemoveAt(0);
                        foundChunkToRemesh = true;
                    }
                }

                if(foundChunkToRemesh)
                {
                    if(toRenderChunks.TryGetValue(chunkMesh.chunkGridPosition, out RenderChunk renderChunk))
                    {
                        renderChunk.HardBlocksModel.CleanUp();
                    } else
                    {
                        renderChunk = new RenderChunk((int)chunkMesh.chunkGridPosition.X, (int)chunkMesh.chunkGridPosition.Y);
                        toRenderChunks.Add(chunkMesh.chunkGridPosition, renderChunk);
                    }

                    renderChunk.HardBlocksModel = new VAOModel(chunkMesh.chunkLayout);
                } else
                {
                    break;
                }
            }
        }

        public void OnChunkLoaded(World world, Chunk chunk)
        {
            if(toRenderChunks.ContainsKey(new Vector2(chunk.GridX, chunk.GridZ)))
                 throw new Exception();

            foreach(Chunk editedLightMapChunk in FloodFillLight.GenerateInitialSunlightGrid(world, chunk))
            {
                MeshChunk(editedLightMapChunk);
            }

            foreach(KeyValuePair<Vector3i, BlockState> kp in chunk.LightSourceBlocks)
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, chunk, kp.Key, kp.Value))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }

            /* if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXNeg))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXPos))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZNeg))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZPos))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }

             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ - 1), out Chunk cXNeg1))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXNeg1))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ + 1), out Chunk cXPos2))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXPos2))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ + 1), out Chunk cZNeg3))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZNeg3))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ - 1), out Chunk cZPos4))
             {
                 foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZPos4))
                 {
                     MeshChunk(editedLightMapChunk);
                 }
             }*/

            /* if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg)
                && cXNeg.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cXNeg.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cXNeg, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos)
                 && cXPos.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cXPos.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cXPos, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg)
                && cZNeg.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cZNeg.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cZNeg, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos)
                && cZPos.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cZPos.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cZPos, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }

             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ - 1), out Chunk cXNeg1)
                && cXNeg1.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cXNeg1.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cXNeg1, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ + 1), out Chunk cXPos2)
                && cXPos2.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cXPos2.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cXPos2, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ + 1), out Chunk cZNeg3)
                 && cZNeg3.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cZNeg3.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cZNeg3, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ - 1), out Chunk cZPos4)
                && cZPos4.LightSourceBlocks.Count != 0)
             {
                 foreach(KeyValuePair<Vector3i, BlockState> kp in cZPos4.LightSourceBlocks)
                 {
                     foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, cZPos4, kp.Key, kp.Value))
                     {
                         MeshChunk(editedLightMapChunk);
                     }
                 }
             }
             */
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
            Vector2 chunkPos = new Vector2(chunk.GridX, chunk.GridZ);
            lock (meshLock)
            {
                if(toRemeshChunksSet.Remove(chunk))
                {
                    if(!toRemeshChunksQueue.Remove(chunk))
                        throw new Exception();
                }
                availableChunkMeshes.RemoveAll(m => m.chunkGridPosition == chunkPos);
            }
            toRenderChunks.Remove(chunkPos);
            MeshNeighbourChunks(world, chunk);
        }

        public void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {                
            foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockAdded(world, chunk, blockPos, newState))
            {
                MeshChunk(editedLightMapChunk);
            }

            foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGridOnBlockAdded(world, chunk, blockPos, newState))
            {
                MeshChunk(editedLightMapChunk);
            }

          /*  if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXNeg))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXPos))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZNeg))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZPos))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }

            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ - 1), out Chunk cXNeg1))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXNeg1))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ + 1), out Chunk cXPos2))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cXPos2))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ + 1), out Chunk cZNeg3))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZNeg3))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }
            if(world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ - 1), out Chunk cZPos4))
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGrid(world, cZPos4))
                {
                    MeshChunk(editedLightMapChunk);
                }
            }*/

            MeshChunkAndSurroundings(world, chunk, blockPos, newState);
        }

        public void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, int chainPos, int chainCount)
        {
            if(chainPos == chainCount)
            {
                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairLightGridBlockRemoved(world, chunk, blockPos))
                {
                    MeshChunk(editedLightMapChunk);
                }

                foreach(Chunk editedLightMapChunk in FloodFillLight.RepairSunlightGridBlockRemoved(world, chunk, blockPos))
                {
                    MeshChunk(editedLightMapChunk);
                }
                MeshChunkAndSurroundings(world, chunk, blockPos, oldState);
            }
        }

        private void MeshChunk(Chunk chunk)
        {
            lock (meshLock)
            {
                if (!toRemeshChunksSet.Contains(chunk))
                {
                    toRemeshChunksSet.Add(chunk);
                    toRemeshChunksQueue.AddLast(chunk);
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

            foreach(KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                chunkToRender.Value.CleanUp();
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
