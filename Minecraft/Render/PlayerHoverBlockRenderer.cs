using OpenTK;

namespace Minecraft
{
    class PlayerHoverBlockRenderer
    {
        private WireframeRenderer wireframeRenderer;
        private ClientPlayer player;

        public PlayerHoverBlockRenderer(WireframeRenderer wireframeRenderer, ClientPlayer player)
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
            Vector3 translation = player.mouseOverObject.intersectedBlockPos.ToFloat() - new Vector3(offset, offset, offset);
            wireframeRenderer.RenderWireframeAt(3, translation, scaleVector);
        }
    }
}