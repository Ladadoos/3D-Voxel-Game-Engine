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
                _text = value;
                parentCanvas.AddComponentToClean(this);
            }
        }

        private Font font;
        private VAOModel vaoModel;

        public UIText(UICanvas parentCanvas, Font font, Vector2 position, string text) : base(parentCanvas, position)
        {
            this.font = font;
            this.text = text;
        }

        public override void Clean()
        {
            float[] vertices = font.GetVerticesForText(this);
            float[] textures = font.GetTexturesForText(this);
            int indicesCount = text.Length * 6;
            vaoModel = new VAOModel(vertices, textures, indicesCount);
        }

        public override void Render(UIShader uiShader)
        {
            vaoModel.Bind();
            uiShader.LoadTexture(uiShader.location_Texture, 0, font.fontMap.textureId);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vaoModel.indicesCount);
        }
    }
}
