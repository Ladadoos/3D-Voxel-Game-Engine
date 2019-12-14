using OpenTK;

namespace Minecraft
{
    class PlayerHoverBlockRenderer
    {
        private WireframeRenderer wireframeRenderer;
        private Player player;

        public PlayerHoverBlockRenderer(WireframeRenderer wireframeRenderer, Player player)
        {
            this.wireframeRenderer = wireframeRenderer;
            this.player = player;
        }

        public void RenderSelection()
        {
            if(player.mouseOverObject == null)
            {
                return;
            }

            float scale = 1.001f;
            float offset = (scale - 1) / 2; 
            Vector3 scaleVector = new Vector3(scale, scale, scale);
            Vector3 translation = player.mouseOverObject.intersectedGridPoint - new Vector3(offset, offset, offset);
            wireframeRenderer.RenderWireframeAt(3, translation, scaleVector);
        }
    }
}