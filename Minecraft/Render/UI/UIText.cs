using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class UIText : UIComponent
    {
        private string _text;
        public string text {
            get {
                return _text;
            }
            set {
                if(_text == value)
                {
                    return;
                }

                _text = value;
                parentCanvas.AddComponentToClean(this);
            }
        }

        public Font font { get; private set; }
        public Vector2 scale { get; private set; }

        private TextMeshBuilder meshBuilder = new TextMeshBuilder();

        public UIText(UICanvas parentCanvas, Font font, Vector2 position, Vector2 scale, string text) : base(parentCanvas, position)
        {
            this.font = font;
            this.text = text;
            this.scale = scale;
        }

        public override void Clean()
        {
            float[] vertices = meshBuilder.GetVerticesForText(this);
            float[] textures = meshBuilder.GetTexturesForText(this);
            int indicesCount = vertices.Length / 3;
            vaoModel?.OnCloseGame();
            vaoModel = new VAOModel(vertices, textures, indicesCount);
        }

        public override void Render(UIShader uiShader)
        {
            vaoModel.Bind();
            uiShader.LoadTexture(uiShader.location_Texture, 0, font.fontMapTexture.textureId);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vaoModel.indicesCount);
        }
    }
}
