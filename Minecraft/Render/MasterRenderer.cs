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

        public MasterRenderer(Game game)
        {
            playerCamera = game.player.camera;
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
            foreach (KeyValuePair<Vector2, RenderChunk> renderChunk in world.renderChunks)
            {
                Vector3 min = new Vector3(renderChunk.Key.X * 16, 0, renderChunk.Key.Y * 16);
                Vector3 max = min + new Vector3(16, 256, 16);
                if (!playerCamera.viewFrustum.IsAABBInFrustum(new AABB(min, max)))
                {
                    continue;
                }
                renderChunk.Value.HardBlocksModel.Bind();
                basicShader.LoadMatrix(basicShader.location_TransformationMatrix, renderChunk.Value.TransformationMatrix);
                GL.DrawElements(PrimitiveType.Triangles, renderChunk.Value.HardBlocksModel.indicesCount, DrawElementsType.UnsignedInt, 0);
                renderChunk.Value.HardBlocksModel.Unbind();
            }
            basicShader.Stop();

            playerBlockRenderer.RenderSelection();
        }

        public void OnPlayerCameraProjectionChanged(ProjectionMatrixInfo pInfo)
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

        public void CleanUp()
        {
            basicShader.CleanUp();
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
