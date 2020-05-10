﻿using OpenTK;

namespace Minecraft
{
    class PlayerHoverBlockRenderer
    {
        private readonly WireframeRenderer wireframeRenderer;
        private readonly ClientPlayer player;

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

            Vector3i blockPos = player.mouseOverObject.IntersectedBlocKPos;
            BlockState state = player.mouseOverObject.BlockstateHit;

            float offset = 0.001f;
            foreach(AxisAlignedBox aabb in state.GetBlock().GetSelectionBox(state, blockPos))
            {
                Vector3 scaleVector = new Vector3(aabb.Max.X - aabb.Min.X, aabb.Max.Y - aabb.Min.Y, aabb.Max.Z - aabb.Min.Z);
                Vector3 translation = aabb.Min;
                wireframeRenderer.RenderWireframeAt(3, translation, scaleVector, new Vector3(offset, offset, offset));
            }
        }
    }
}