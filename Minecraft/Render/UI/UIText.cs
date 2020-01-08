using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class UIText : UIComponent
    {
        private UICanvas canvas;
        private Font font;
        public string text { get; private set; }

        private VAOModel vaoModel;

        public UIText(UICanvas canvas, Font font, Vector2 position, string text) : base(position)
        {
            this.canvas = canvas;
            this.font = font;
            this.text = text;

            float[] vertices = font.GetVerticesScreenSpace(this, canvas);
            float[] textures = font.GetTexturesScreenSpace(this, canvas);
            int indicesCount = text.Length * 6;
            vaoModel = new VAOModel(vertices, textures, indicesCount);
        }

        public override void Render(UIShader uiShader)
        {
            vaoModel.Bind();
            uiShader.LoadTexture(uiShader.location_Texture, 0, font.fontMap.textureId);
            uiShader.LoadMatrix(uiShader.location_TransformationMatrix, Matrix4.Identity * Matrix4.CreateScale(7));
            GL.DrawArrays(PrimitiveType.Triangles, 0, vaoModel.indicesCount);
        }
    }
}
