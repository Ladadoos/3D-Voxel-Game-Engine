using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class UIText : UIComponent
    {
        private string text;
        public string Text {
            get {
                return text;
            }
            set {
                if(text == value)
                {
                    return;
                }

                text = value;
                ParentCanvas.AddComponentToClean(this);
            }
        }

        public Font Font { get; private set; }
        public Vector2 Scale { get; private set; }

        private readonly TextMeshBuilder meshBuilder = new TextMeshBuilder();

        public UIText(UICanvas parentCanvas, Font font, Vector2 position, Vector2 scale, string text) : base(parentCanvas, position)
        {
            Font = font;
            Text = text;
            Scale = scale;
        }

        public override void Clean()
        {
            float[] vertices = meshBuilder.GetVerticesForText(this);
            float[] textures = meshBuilder.GetTexturesForText(this);
            int indicesCount = vertices.Length / 3;
            vaoModel?.CleanUp();
            vaoModel = new VAOModel(vertices, textures, indicesCount);
        }

        public override void Render(UIShader uiShader)
        {
            base.Render(uiShader);
            vaoModel.BindVAO();
            uiShader.LoadTexture(uiShader.Location_Texture, 0, Font.FontMapTexture.ID);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vaoModel.IndicesCount);
        }
    }
}
