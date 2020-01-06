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

            float offset = 0.001f;
            Vector3 scaleVector = new Vector3(1, 1, 1);
            Vector3 translation = player.mouseOverObject.intersectedBlockPos.ToFloat();
            wireframeRenderer.RenderWireframeAt(3, translation, scaleVector, new Vector3(offset, offset, offset));
        }
    }
}