using OpenTK;
using System;

namespace Minecraft
{
    class DebugHelper
    {
        private WireframeRenderer wireframeRenderer;
        private Game game;

        private UICanvasDebug debugCanvas;

        private bool renderHitboxes;
        private bool renderChunkBorders;
        private bool displayDebugInfo;

        public DebugHelper(Game game, WireframeRenderer wireframeRenderer)
        {
            this.wireframeRenderer = wireframeRenderer;
            this.game = game;
        }

        public void UpdateAndRender()
        {
            if (Game.input.OnKeyPress(OpenTK.Input.Key.F1))
            {
                renderHitboxes = !renderHitboxes;
            }else if (Game.input.OnKeyPress(OpenTK.Input.Key.F2) && game.mode != RunMode.Server)
            {
                displayDebugInfo = !displayDebugInfo;
                if (displayDebugInfo)
                {
                    debugCanvas = new UICanvasDebug(game);
                    game.masterRenderer.AddCanvas(debugCanvas);
                } else
                {
                    game.masterRenderer.RemoveCanvas(debugCanvas);
                }
            }else if (Game.input.OnKeyPress(OpenTK.Input.Key.F3))
            {
                GC.Collect();
                Logger.Info("Manual garbage collected.");
            } else if (Game.input.OnKeyPress(OpenTK.Input.Key.F4))
            {
                for (int x = -3; x < 3; x++)
                    for (int y = -3; y < 3; y++)
                        for (int z = -3; z < 3; z++)
                            game.client.WritePacket(new RemoveBlockPacket(new Vector3i(x, y, z) + new Vector3i(game.player.position)));
            } else if (Game.input.OnKeyPress(OpenTK.Input.Key.F5))
            {
                renderChunkBorders = !renderChunkBorders;
            }

            Render();
        }

        private void Render()
        {
            if (renderHitboxes)
            {
                foreach(Entity entity in game.world.loadedEntities.Values)
                {
                    AxisAlignedBox aabb = entity.hitbox;
                    float width = Math.Abs(aabb.max.X - aabb.min.X);
                    float length = Math.Abs(aabb.max.Z - aabb.min.Z);
                    float height = Math.Abs(aabb.max.Y - aabb.min.Y);

                    float offset = 0.001f;
                    Vector3 scaleVector = new Vector3(width, height, length);
                    Vector3 translation = entity.position;
                    wireframeRenderer.RenderWireframeAt(2, translation, scaleVector, new Vector3(offset, offset, offset));
                }
            }

            if (renderChunkBorders)
            {
                game.masterRenderer.RenderChunkBorders();
            }
        }
    }
}