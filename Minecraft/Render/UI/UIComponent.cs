using OpenTK;

namespace Minecraft
{
    abstract class UIComponent
    {
        public UICanvas ParentCanvas { get; private set; }
        public Vector2 PixelPositionInCanvas { get; private set; }
        protected VAOModel vaoModel;

        protected UIComponent(UICanvas parentCanvas, Vector2 pixelPositionInCanvas)
        {
            ParentCanvas = parentCanvas;
            PixelPositionInCanvas = pixelPositionInCanvas;
        }

        public abstract void Clean();

        public abstract void Render(UIShader uiShader);
    }
}
