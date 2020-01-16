using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class WireframeRenderer
    {
        private WireframeShader shader = new WireframeShader();
        private Camera camera;
        private VAOModel aabbCube;

        public WireframeRenderer(Camera camera)
        {
            this.camera = camera;
            CreateDefaultAABBCube();
        }

        private void CreateDefaultAABBCube()
        {
            //Direction are relative to facing positive Z
            float[] positions = new float[] {
                //Bottom points (looking from top down)
                0, 0, 0, //Bottom-right   -- index 0
                1, 0, 0, //Bottom-left    -- index 1
                1, 0, 1, //Top-left       -- index 2
                0, 0, 1, //Top-right      -- index 3
                //Top points    (looking from top down)
                0, 1, 0, //Bottom-right   -- index 4
                1, 1, 0, //Bottom-left    -- index 5
                1, 1, 1, //Top-left       -- index 6
                0, 1, 1, //Top-right      -- index 7
            };
            int[] indices = new int[] {
                0, 1, 2, 3, //Bottom
                7, 6, 5, 4, //Top
                3, 7, 4, 0, //Right
                2, 1, 5, 6, //Left
                0, 4, 5, 1, //Front
                3, 2, 6, 7  //Back
            };
            aabbCube = new VAOModel(positions, indices);
        }

        /// <summary> Draws a cube wireframe at the given location. Scale is relative to a 1x1x1 cube. </summary>
        public void RenderWireframeAt(int lineWidth, Vector3 translation, Vector3 scale)
        {
            GL.LineWidth(lineWidth);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            shader.Start();
            shader.LoadMatrix(shader.location_ViewMatrix, camera.currentViewMatrix);
            shader.LoadMatrix(shader.location_ProjectionMatrix, camera.currentProjectionMatrix);

            Matrix4 transformMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(translation);
            shader.LoadMatrix(shader.location_TransformationMatrix, Matrix4.Identity * transformMatrix);
            aabbCube.BindVAO();
            GL.DrawElements(PrimitiveType.Quads, aabbCube.indicesCount, DrawElementsType.UnsignedInt, 0);
            aabbCube.UnbindVAO();
            shader.Stop();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        /// <summary> Draws a cube wireframe at the given location. Scale is relative to a 1x1x1 cube. </summary>
        public void RenderWireframeAt(int lineWidth, Vector3 translation, Vector3 scale, Vector3 offset)
        {
            scale += offset;
            translation -= (offset / 2);
            RenderWireframeAt(lineWidth, translation, scale);
        }

        public void OnCloseGame()
        {
            aabbCube.OnCloseGame();
        }
    }
}
