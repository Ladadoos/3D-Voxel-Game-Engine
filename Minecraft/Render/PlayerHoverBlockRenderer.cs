using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class PlayerHoverBlockRenderer
    {
        private SelectedBlockShader blockShader;
        private Player player;
        private Model cube;

        public PlayerHoverBlockRenderer(Player player)
        {
            this.player = player;
            blockShader = new SelectedBlockShader();

            //Direction are relative to facing positive Z
            float[] positions = new float[] {
                //Bottom points (looking from top down)
                0, 0, 0, //Bottom-left    -- index 0
                1, 0, 0, //Bottom-right   -- index 1
                1, 0, 1, //Top-right      -- index 2
                0, 0, 1, //Top-left       -- index 3
                //Top points    (looking from top down)
                0, 1, 0, //Bottom-left    -- index 4
                1, 1, 0, //Bottom-right   -- index 5
                1, 1, 1, //Top-right      -- index 6
                0, 1, 1, //Top-left       -- index 7
            };
            int[] indices = new int[] {
                0, 1, 2, 3, //Bottom
                7, 6, 5, 4, //Top
                3, 7, 4, 0, //Right
                2, 1, 5, 6, //Left
                0, 4, 5, 1, //Front
                3, 2, 6, 7  //Back
            };
            cube = new Model(positions, indices);
        }

        public void RenderSelection(Matrix4 playerViewMatrix)
        {
            if(player.mouseOverObject == null)
            {
                return;
            }

            float scale = 1.001f;
            float size = (scale - 1) / 2;

            GL.LineWidth(3);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            blockShader.Start();
            blockShader.LoadMatrix(blockShader.location_ViewMatrix, playerViewMatrix);
            blockShader.LoadMatrix(blockShader.location_ProjectionMatrix, player.camera.currentProjectionMatrix);

            Matrix4 transformMatrix = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(player.mouseOverObject.intersectedGridPoint - new Vector3(size, size, size));
            blockShader.LoadMatrix(blockShader.location_TransformationMatrix, Matrix4.Identity * transformMatrix);
            cube.Bind();
            GL.DrawElements(PrimitiveType.Quads, cube.indicesCount, DrawElementsType.UnsignedInt, 0);
            cube.Unbind();
            blockShader.Stop();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
    }
}