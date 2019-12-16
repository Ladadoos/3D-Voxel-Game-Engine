using OpenTK;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;
using System;

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
        private BlockModelRegistry blockModelRegistry;
        private TextureLoader textureLoader;
        private ScreenQuad screenQuad;

        private Dictionary<Vector2, RenderChunk> toRenderChunks = new Dictionary<Vector2, RenderChunk>();
        private HashSet<ChunkLayer> toRemeshChunks = new HashSet<ChunkLayer>();
        private OpaqueMeshGenerator staticBlocksMeshGenerator;
        private FaunaMeshGenerator faunaMeshGenerator;

        struct ChunkLayer
        {
            public Chunk chunk;
            public BlockMaterial layer;
        }

        public MasterRenderer(Game game)
        {
            playerCamera = game.player.camera;

            textureLoader = new TextureLoader();
            int textureAtlasId = textureLoader.LoadTexture("../../Resources/texturePack.png");
            textureAtlas = new TextureAtlas(textureAtlasId, 256, 16);
            blockModelRegistry = new BlockModelRegistry(textureAtlas);
            staticBlocksMeshGenerator = new OpaqueMeshGenerator(blockModelRegistry);
            faunaMeshGenerator = new FaunaMeshGenerator(blockModelRegistry);
            screenQuad = new ScreenQuad(game.window);
            wireframeRenderer = new WireframeRenderer(game.player.camera);
            playerBlockRenderer = new PlayerHoverBlockRenderer(wireframeRenderer, game.player);

            basicShader = new ShaderBasic();
            UploadProjectionMatrix();
            playerCamera.OnProjectionChangedHandler += OnPlayerCameraProjectionChanged;

            EnableDepthTest();
            EnableCulling();
        }

        public void Render(World world)
        {
            EnableDepthTest();
            screenQuad.fbo.BindFBO();
            GL.ClearColor(colorClearR, colorClearG, colorClearB, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            bool[] renderFlags = new bool[toRenderChunks.Count];
            int i = 0;

            basicShader.Start();
            basicShader.LoadTexture(basicShader.location_TextureAtlas, 0, textureAtlas.textureId);
            basicShader.LoadMatrix(basicShader.location_ViewMatrix, playerCamera.currentViewMatrix);
            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                Vector3 min = new Vector3(chunkToRender.Key.X * 16, 0, chunkToRender.Key.Y * 16);
                Vector3 max = min + new Vector3(16, 256, 16);
                renderFlags[i++] = playerCamera.viewFrustum.IsAABBInFrustum(new AABB(min, max));
                if (!renderFlags[i - 1])
                {
                    continue;
                }
                chunkToRender.Value.hardBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, chunkToRender.Value.transformationMatrix);
                GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.hardBlocksModel.indicesCount);
            }

            i = 0;
            DisableCulling();
            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in toRenderChunks)
            {
                if (!renderFlags[i++])
                {
                    continue;
                }
                chunkToRender.Value.faunaBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, chunkToRender.Value.transformationMatrix);
                GL.DrawArrays(PrimitiveType.Quads, 0, chunkToRender.Value.faunaBlocksModel.indicesCount);
            }
            EnableCulling();

            playerBlockRenderer.RenderSelection();
            screenQuad.fbo.UnbindFBO();
            GL.Disable(EnableCap.DepthTest);
            screenQuad.RenderToScreen();
        }


        private bool firstPass = false;

        public void EndFrameUpdate(World world)
        {
            var start = DateTime.Now;

            foreach (ChunkLayer chunkLayer in toRemeshChunks)
            {
                Vector2 gridPosition = new Vector2(chunkLayer.chunk.gridX, chunkLayer.chunk.gridZ);

                if (!toRenderChunks.TryGetValue(gridPosition, out RenderChunk renderChunk))
                {
                    renderChunk = new RenderChunk(chunkLayer.chunk.gridX, chunkLayer.chunk.gridZ);
                    toRenderChunks.Add(gridPosition, renderChunk);
                }

                if (faunaMeshGenerator.ShouldProcessLayer(chunkLayer.layer))
                {
                    //System.Console.WriteLine("Reconstructing fauna for chunk " + gridPosition);
                    renderChunk.faunaBlocksModel = faunaMeshGenerator.GenerateMeshFor(world, chunkLayer.chunk);
                } else if (staticBlocksMeshGenerator.ShouldProcessLayer(chunkLayer.layer))
                {
                    //System.Console.WriteLine("Reconstructing static blocks for chunk " + gridPosition);
                    renderChunk.hardBlocksModel = staticBlocksMeshGenerator.GenerateMeshFor(world, chunkLayer.chunk);
                }
            }

            var now2 = DateTime.Now - start;
            if (!firstPass)
            {
                Console.WriteLine("Generating init meshes took: " + now2 + " s");
            }

            toRemeshChunks.Clear();
            firstPass = true;
        }

        public void OnChunkLoaded(Chunk chunk)
        {
            MeshChunk(new ChunkLayer { chunk = chunk, layer = BlockMaterial.Opaque });
            MeshChunk(new ChunkLayer { chunk = chunk, layer = BlockMaterial.Fauna });
        }

        public void OnBlockPlaced(World world, Chunk chunk, BlockState oldState, BlockState newState)
        {
            if ((oldState.block.material == BlockMaterial.Fauna && newState.block.material == BlockMaterial.Air)
                || (newState.block.material == BlockMaterial.Fauna))
            {
                MeshChunk(new ChunkLayer { chunk = chunk, layer = BlockMaterial.Fauna });
            } else
            {
                int localX = (int)newState.position.X & 15;
                int localZ = (int)newState.position.Z & 15;
                MeshChunk(new ChunkLayer { chunk = chunk, layer = BlockMaterial.Opaque });

                if (localX == 0 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg))
                {
                    MeshChunk(new ChunkLayer { chunk = cXNeg, layer = BlockMaterial.Opaque });
                }
                if (localX == 15 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos))
                {
                    MeshChunk(new ChunkLayer { chunk = cXPos, layer = BlockMaterial.Opaque });
                }
                if (localZ == 0 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg))
                {
                    MeshChunk(new ChunkLayer { chunk = cZNeg, layer = BlockMaterial.Opaque });
                }
                if (localZ == 15 && world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos))
                {
                    MeshChunk(new ChunkLayer { chunk = cZPos, layer = BlockMaterial.Opaque });
                }
            }
        }

        private void MeshChunk(ChunkLayer chunkLayer)
        {
            if (!toRemeshChunks.Contains(chunkLayer))
            {
                toRemeshChunks.Add(chunkLayer);
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
