using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class MasterRenderer
    {
        private float colorClearR = 0.57F;
        private float colorClearG = 0.73F;
        private float colorClearB = 1.0F;

        private ShaderBasic basicShader;
        private Camera playerCamera;
        private WireframeRenderer wireframeRenderer;
        private PlayerHoverBlockRenderer playerBlockRenderer;
        private TextureAtlas textureAtlas;
        public BlockModelRegistry blockModelRegistry { get; private set; }
        private TextureLoader textureLoader;

        private Dictionary<Vector2, RenderChunk> toRenderChunks = new Dictionary<Vector2, RenderChunk>();
        private HashSet<Chunk> toRemeshChunks = new HashSet<Chunk>();
        private ChunkMeshGenerator staticBlocksMeshGenerator;

        public MasterRenderer(Game game)
        {
            playerCamera = game.player.camera;

            textureLoader = new TextureLoader();
            int textureAtlasId = textureLoader.LoadTexture("../../Resources/texturePack.png");
            textureAtlas = new TextureAtlas(textureAtlasId, 256, 16);
            blockModelRegistry = new BlockModelRegistry(textureAtlas);
            staticBlocksMeshGenerator = new ChunkMeshGenerator();

            wireframeRenderer = new WireframeRenderer(game.player.camera);
            playerBlockRenderer = new PlayerHoverBlockRenderer(wireframeRenderer, game.player);

            basicShader = new ShaderBasic();
            UploadTextureAtlas();
            UploadProjectionMatrix();
            playerCamera.OnProjectionChangedHandler += OnPlayerCameraProjectionChanged;

            EnableDepthTest();
            EnableCulling();
        }

        public void Render(World world)
        {     
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
   
            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, playerCamera.currentViewMatrix);
            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                Vector3 max = min + new Vector3(16, 256, 16);
                if (!playerCamera.viewFrustum.IsAABBInFrustum(new AABB(min, max)))
                {
                    continue;
                }
                chunkToRender.Value.hardBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, chunkToRender.Value.transformationMatrix);
                GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.hardBlocksModel.indicesCount);
                chunkToRender.Value.hardBlocksModel.Unbind();
            }
            basicShader.Stop();

            playerBlockRenderer.RenderSelection();
        }

        public void EndFrameUpdate(World world)
        {
            foreach(Chunk chunk in toRemeshChunks)
            {
                staticBlocksMeshGenerator.GenerateRenderMeshForChunk(world, this, chunk);
            }
            toRemeshChunks.Clear();
        }

        public void OnChunkLoaded(Chunk chunk)
        {
            MeshChunk(chunk);
        }

        public void AddChunkToRender(RenderChunk renderChunk)
        {
            if (toRenderChunks.ContainsKey(renderChunk.gridPosition))
            {
                toRenderChunks[renderChunk.gridPosition] = renderChunk;
            } else
            {
                toRenderChunks.Add(renderChunk.gridPosition, renderChunk);
            }
        }

        public void OnBlockPlaced(World world, Chunk chunk, BlockState blockstate)
        {
            int localX = (int)blockstate.position.X & 15;
            int localZ = (int)blockstate.position.Z & 15;
            MeshChunk(chunk);

            if (localX == 0)
            {
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg);
                if (cXNeg != null)
                {
                    MeshChunk(cXNeg);
                }
            }

            if (localX == 15)
            {
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos);
                if (cXPos != null)
                {
                    MeshChunk(cXPos);
                }
            }

            if (localZ == 0)
            {
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg);
                if (cZNeg != null)
                {
                    MeshChunk(cZNeg);
                }
            }

            if (localZ == 15)
            {
                world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos);
                if (cZPos != null)
                {
                    MeshChunk(cZPos);
                }
            }
        }

        private void MeshChunk(Chunk chunk)
        {
            if (!toRemeshChunks.Contains(chunk))
            {
                toRemeshChunks.Add(chunk);
            }
        }

        private void OnPlayerCameraProjectionChanged(ProjectionMatrixInfo pInfo)
        {    
            UploadProjectionMatrix();
        }

        private void UploadProjectionMatrix()
        {
            basicShader.Start();
            basicShader.LoadMatrix(basicShader.location_ProjectionMatrix, playerCamera.currentProjectionMatrix);
            basicShader.Stop();
        }

        private void UploadTextureAtlas()
        {
            basicShader.Start();
            basicShader.LoadInt(basicShader.location_TextureAtlas, 0);
            basicShader.Stop();
        }

        public void OnCloseGame()
        {
            basicShader.OnCloseGame();
            textureLoader.OnCloseGame();
            wireframeRenderer.OnCloseGame();

            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                chunkToRender.Value.OnCloseGame();
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
