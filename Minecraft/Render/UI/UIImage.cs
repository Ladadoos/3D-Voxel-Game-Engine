using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class UIImage : UIComponent
    {
        private Texture texture;
        public Texture Texture {
            get { return texture; }
            set {
                if(texture?.ID == value.ID)
                    return;

                texture = value;
                ParentCanvas.AddComponentToClean(this);
            }
        }

        private Vector2 dimension;

        public UIImage(UICanvas parentCanvas, Vector2 position, Vector2 dimension, Texture texture) : base(parentCanvas, position)
        {
            this.Texture = texture;
            this.dimension = dimension;
        }

        public override void Clean()
        {
            float xNdc = (PixelPositionInCanvas.X / ParentCanvas.PixelWidth) * 2 - 1;
            float yNdc = 1 - (PixelPositionInCanvas.Y / ParentCanvas.PixelHeight) * 2;
            float cwidth = 2 * dimension.X / ParentCanvas.PixelWidth;
            float cHeight = 2 * -dimension.Y / ParentCanvas.PixelHeight;
            Vector3 topLeft = new Vector3(xNdc, yNdc, 0);
            Vector3 bottomLeft = new Vector3(xNdc, yNdc + cHeight, 0);
            Vector3 bottomRight = new Vector3(xNdc + cwidth, yNdc + cHeight, 0);
            Vector3 topRight = new Vector3(xNdc + cwidth, yNdc, 0);

            float[] vertices = new float[18];
            vertices[0] = bottomLeft.X; vertices[1] = bottomLeft.Y; vertices[2] = bottomLeft.Z;  //bottom-left
            vertices[3] = bottomRight.X; vertices[4] = bottomRight.Y; vertices[5] = bottomRight.Z; //bottom-right
            vertices[6] = topRight.X; vertices[7] = topRight.Y; vertices[8] = topRight.Z;    //top-right
            vertices[9] = bottomLeft.X; vertices[10] = bottomLeft.Y; vertices[11] = bottomLeft.Z;  //bottom-left
            vertices[12] = topRight.X; vertices[13] = topRight.Y; vertices[14] = topRight.Z;    //top-right
            vertices[15] = topLeft.X; vertices[16] = topLeft.Y; vertices[17] = topLeft.Z;     //top-left

            float[] textures = new float[12];
            int i = 0;
            textures[0] = 0;  textures[i + 1] = 1; //bottom-left
            textures[2] = 1;  textures[i + 3] = 1; //bottom-right
            textures[4] = 1;  textures[i + 5] = 0; //top-right
            textures[6] = 0;  textures[i + 7] = 1; //bottom-left
            textures[8] = 1;  textures[i + 9] = 0; //top-right
            textures[10] = 0; textures[11] = 0;    //top-left

            vaoModel = new VAOModel(vertices, textures, 6);
        }

        public override void Render(UIShader uiShader)
        {
            base.Render(uiShader);
            vaoModel.BindVAO();
            uiShader.LoadTexture(uiShader.Location_Texture, 0, Texture.ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vaoModel.IndicesCount);
        }
    }
}
