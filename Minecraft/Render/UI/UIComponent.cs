using OpenTK;

namespace Minecraft
{
    abstract class UIComponent
    {
        public Vector2 positionInCanvas;

        public UIComponent(Vector2 positionInCanvas)
        {
            this.positionInCanvas = positionInCanvas;
        }

        public abstract void Render(UIShader uiShader);
    }
}
