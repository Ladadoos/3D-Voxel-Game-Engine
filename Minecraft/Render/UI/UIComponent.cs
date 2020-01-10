using OpenTK;

namespace Minecraft
{
    abstract class UIComponent
    {
        public UICanvas parentCanvas { get; private set; }
        public Vector2 pixelPositionInCanvas { get; private set; }
        protected VAOModel vaoModel;

        public UIComponent(UICanvas parentCanvas, Vector2 pixelPositionInCanvas)
        {
            this.parentCanvas = parentCanvas;
            this.pixelPositionInCanvas = pixelPositionInCanvas;
        }

        public abstract void Clean();

        public abstract void Render(UIShader uiShader);
    }
}
