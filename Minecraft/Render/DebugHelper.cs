﻿using OpenTK;
using System;

namespace Minecraft
{
    class DebugHelper
    {
        public readonly LightDebugRenderer lightDebug;
        private readonly WireframeRenderer wireframeRenderer;
        private readonly Game game;
        private readonly Camera debugCamera;
        private UICanvasDebug debugCanvas;

        private bool renderFromPlayerCamera;
        private bool renderHitboxes;
        private bool renderChunkBorders;
        public bool renderBlockLightAreas;
        private bool displayDebugInfo;

        public DebugHelper(Game game, WireframeRenderer wireframeRenderer)
        {
            this.wireframeRenderer = wireframeRenderer;
            this.game = game;
            this.lightDebug = new LightDebugRenderer(game, wireframeRenderer);

            debugCamera = new Camera(new ProjectionMatrixInfo
            {
                DistanceNearPlane = 0.1F,
                DistanceFarPlane = 1000F,
                FieldOfView = 1.5F,
                WindowPixelWidth = game.Window.Width,
                WindowPixelHeight = game.Window.Height
            });
        }

        public void UpdateAndRender()
        {
            if(Game.Input.OnKeyPress(OpenTK.Input.Key.F1))
            {
                renderHitboxes = !renderHitboxes;
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F2) && game.RunMode != RunMode.Server)
            {
                displayDebugInfo = !displayDebugInfo;
                if(displayDebugInfo)
                {
                    debugCanvas = new UICanvasDebug(game);
                    game.MasterRenderer.AddCanvas(debugCanvas);
                } else
                {
                    game.MasterRenderer.RemoveCanvas(debugCanvas);
                }
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F3))
            {
                GC.Collect();
                Logger.Info("Manual garbage collected.");
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F4))
            {
                Vector3i[] blockPositions = new Vector3i[7 * 7 * 7];
                int i = 0;
                for(int x = -3; x < 3; x++)
                    for(int y = -3; y < 3; y++)
                        for(int z = -3; z < 3; z++)
                            blockPositions[i++] = new Vector3i(x, y, z) + new Vector3i(game.ClientPlayer.Position);
                game.Client.WritePacket(new RemoveBlockPacket(blockPositions));
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F5))
            {
                renderChunkBorders = !renderChunkBorders;
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F6))
            {
                renderFromPlayerCamera = !renderFromPlayerCamera;

                if(!renderFromPlayerCamera)
                {
                    debugCamera.SetPosition(game.ClientPlayer.Position + new Vector3(0, 100, 0));
                    debugCamera.SetYawAndPitch(0, (float)(-Math.PI / 2));
                    game.MasterRenderer.SetActiveCamera(debugCamera);
                } else
                {
                    game.MasterRenderer.SetActiveCamera(game.ClientPlayer.Camera);
                }
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F7))
            {
                renderBlockLightAreas = !renderBlockLightAreas;
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F8))
            {
                for(int x = 0; x < 16; x++)
                {
                    for(int z = 0; z < 16; z++)
                    {
                        Vector2 chunkPos = World.GetChunkPosition(game.ClientPlayer.Position.X, game.ClientPlayer.Position.Z);
                        if(game.World.LoadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                        {
                            game.Client.WritePacket(new PlaceBlockPacket(Blocks.GetState(Blocks.Tnt),
                             new Vector3i(x + 16 * chunk.GridX, (int)game.ClientPlayer.Position.Y + 4, z + 16 * chunk.GridZ)));
                        }                       
                    }
                }
            } else if(Game.Input.OnKeyPress(OpenTK.Input.Key.F9))
            {
                for(int x = 0; x < 8; x++)
                {
                    for(int z = 0; z < 8; z++)
                    {
                        game.Client.WritePacket(new PlaceBlockPacket(Blocks.GetState(Blocks.Grass),
                             new Vector3i(x, 2, z) + new Vector3i(game.ClientPlayer.Position)));
                    }
                }

                for(int x = 0; x < 8; x++)
                {
                    for(int z = 0; z < 8; z++)
                    {
                        for(int y = 1; y < 7; y++)
                        {
                            if(x == 0 || z == 0 || x == 7 || z == 7)
                            {
                                game.Client.WritePacket(new PlaceBlockPacket(Blocks.GetState(Blocks.Grass),
                                 new Vector3i(x, 2 + y, z) + new Vector3i(game.ClientPlayer.Position)));
                            }
                        }
                    }
                }

                for(int x = 0; x < 8; x++)
                {
                    for(int z = 0; z < 8; z++)
                    {
                        game.Client.WritePacket(new PlaceBlockPacket(Blocks.GetState(Blocks.Grass),
                             new Vector3i(x, 9, z) + new Vector3i(game.ClientPlayer.Position)));
                    }
                }

            }

            Render();
        }

        private void Render()
        {
            if (renderHitboxes)
            {
                foreach(Entity entity in game.World.LoadedEntities.Values)
                {
                    AxisAlignedBox aabb = entity.Hitbox;
                    float width = Math.Abs(aabb.Max.X - aabb.Min.X);
                    float length = Math.Abs(aabb.Max.Z - aabb.Min.Z);
                    float height = Math.Abs(aabb.Max.Y - aabb.Min.Y);

                    Vector3 scaleVector = new Vector3(width, height, length);
                    Vector3 translation = entity.Position;
                    wireframeRenderer.RenderWireframeAt(2, translation, scaleVector, new Vector3(0.5F, 0, 0));
                }
            }

            if (renderChunkBorders)
            {
                game.MasterRenderer.RenderChunkBorders();
            }

            if(renderBlockLightAreas)
            {
                lightDebug.RenderLightArea();
            }
        }
    }
}