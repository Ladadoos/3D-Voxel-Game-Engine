using OpenTK;
using System;

namespace Minecraft
{
    class DebugHelper
    {
        private readonly WireframeRenderer wireframeRenderer;
        private readonly Game game;
        private readonly Camera debugCamera;
        private UICanvasDebug debugCanvas;

        private bool renderFromPlayerCamera;
        private bool renderHitboxes;
        private bool renderChunkBorders;
        private bool displayDebugInfo;

        public DebugHelper(Game game, WireframeRenderer wireframeRenderer)
        {
            this.wireframeRenderer = wireframeRenderer;
            this.game = game;

            debugCamera = new Camera(new ProjectionMatrixInfo(0.1F, 1000F, 1.5F, game.window.Width, game.window.Height));
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
                Vector3i[] blockPositions = new Vector3i[7 * 7 * 7];
                int i = 0;
                for(int x = -3; x < 3; x++)
                    for(int y = -3; y < 3; y++)
                        for(int z = -3; z < 3; z++)
                            blockPositions[i++] = new Vector3i(x, y, z) + new Vector3i(game.player.position);
                 game.client.WritePacket(new RemoveBlockPacket(blockPositions));
            } else if (Game.input.OnKeyPress(OpenTK.Input.Key.F5))
            {
                renderChunkBorders = !renderChunkBorders;
            } else if(Game.input.OnKeyPress(OpenTK.Input.Key.F6))
            {
                renderFromPlayerCamera = !renderFromPlayerCamera;

                if(!renderFromPlayerCamera)
                {
                    debugCamera.SetPosition(game.player.position + new Vector3(0, 100, 0));
                    debugCamera.SetYawAndPitch(0, (float)(-Math.PI / 2));
                    game.masterRenderer.SetActiveCamera(debugCamera);
                } else
                {
                    game.masterRenderer.SetActiveCamera(game.player.camera);
                }
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