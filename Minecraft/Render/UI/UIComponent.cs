using OpenTK;

namespace Minecraft
{
    abstract class UIComponent
    {
        public UICanvas ParentCanvas { get; private set; }
        public Vector2 PixelPositionInCanvas { get; private set; }
        public float Transparency { get; set; } = 1.0F;
        public Vector3 Color { get; set; } = Vector3.One;
        protected VAOModel vaoModel;

        protected UIComponent(UICanvas parentCanvas, Vector2 pixelPositionInCanvas)
        {
            ParentCanvas = parentCanvas;
            PixelPositionInCanvas = pixelPositionInCanvas;
        }

        public abstract void Clean();

        public virtual void Render(UIShader uiShader)
        {
            uiShader.LoadFloat(uiShader.Location_Transparency, Transparency);
            uiShader.LoadVector(uiShader.Location_Color, Color);
        }
    }
}
