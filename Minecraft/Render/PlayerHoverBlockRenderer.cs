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

            Vector3i blockPos = player.mouseOverObject.intersectedBlockPos;
            BlockState state = player.mouseOverObject.blockstateHit;

            float offset = 0.001f;
            foreach(AxisAlignedBox aabb in state.GetBlock().GetSelectionBox(state, blockPos))
            {
                Vector3 scaleVector = new Vector3(aabb.max.X - aabb.min.X, aabb.max.Y - aabb.min.Y, aabb.max.Z - aabb.min.Z);
                Vector3 translation = aabb.min;
                wireframeRenderer.RenderWireframeAt(3, translation, scaleVector, new Vector3(offset, offset, offset));
            }
        }
    }
}