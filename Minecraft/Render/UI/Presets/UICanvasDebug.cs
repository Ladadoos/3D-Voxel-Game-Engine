using OpenTK;
using System.Text;

namespace Minecraft
{
    class UICanvasDebug : UICanvas
    {
        private Game game;
        private UIText debugText;

        public UICanvasDebug(Game game) : 
            base(Vector3.Zero, Vector3.Zero, game.Window.Width, game.Window.Height, RenderSpace.Screen)
        {
            this.game = game;

            debugText = new UIText(this, FontRegistry.GetFont(FontType.Arial), new Vector2(0, 0), new Vector2(0.4F, 0.4F), "");
            AddComponentToRender(debugText);
        }

        public override void Update()
        {
            float playerX = game.ClientPlayer.Position.X;
            float playerY = game.ClientPlayer.Position.Y;
            float playerZ = game.ClientPlayer.Position.Z;

            float playerVelX = game.ClientPlayer.Velocity.X;
            float playerVelY = game.ClientPlayer.Velocity.Y;
            float playerVelZ = game.ClientPlayer.Velocity.Z;

            float playerAccelX = game.ClientPlayer.Acceleration.X;
            float playerAccelY = game.ClientPlayer.Acceleration.Y;
            float playerAccelZ = game.ClientPlayer.Velocity.Z;

            Vector3i chunkLocalPos = new Vector3i(game.ClientPlayer.Position).ToChunkLocal();
            Vector2 chunkPos = World.GetChunkPosition(playerX, playerZ);
            Vector3i playerGridPos = new Vector3i(game.ClientPlayer.Position);
            game.World.loadedChunks.TryGetValue(chunkPos, out Chunk currentChunk);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Focused=" + game.Window.Focused + " Vsync=" + game.Window.VSync);
            sb.AppendLine("Position X=" + playerX.ToString("0.00") + " Y=" + playerY.ToString("0.00") + " Z=" + playerZ.ToString("0.00") + 
                          " Grid Position X=" + playerGridPos.X + " Y=" + playerGridPos.Y + " Z= " + playerGridPos.Z);
            sb.AppendLine("Velocity X=" + playerVelX.ToString("0.00") + " Y=" + playerVelY.ToString("0.00") + " Z=" + playerVelZ.ToString("0.00"));
            sb.AppendLine("Acceleration X=" + playerAccelX.ToString("0.00") + " Y=" + playerAccelY.ToString("0.00") + " Z=" + playerAccelZ.ToString("0.00"));       
            sb.AppendLine("Chunk X=" + (int)chunkPos.X + " Z=" + (int)chunkPos.Y);

            if(currentChunk != null)
            {
                sb.AppendLine("Light sources in chunk=" + currentChunk.LightSourceBlocks.Count +
                              " Desired strength=" + game.MasterRenderer.DebugHelper.lightDebug.DesiredLightLevel +
                              " Debug=" + game.MasterRenderer.DebugHelper.renderBlockLightAreas);
            }

            string lightDebug = string.Empty;
            string blockDebug = string.Empty;
            if(chunkLocalPos.Y > 0 && chunkLocalPos.Y < Constants.MAX_BUILD_HEIGHT && currentChunk != null)
            {
                lightDebug += "Light at feet R=" + currentChunk.LightMap.GetRedBlockLightAt(chunkLocalPos) +
                                            " G=" + currentChunk.LightMap.GetGreenBlockLightAt(chunkLocalPos) +
                                            " B=" + currentChunk.LightMap.GetBlueBlockLightAt(chunkLocalPos) +
                                            " Sun=" + currentChunk.LightMap.GetSunLightIntensityAt(chunkLocalPos);
            }
            if(game.ClientPlayer.mouseOverObject != null && currentChunk != null)
            {
                Vector3i intersectedBlockPos = game.ClientPlayer.mouseOverObject.IntersectedBlockPos;
                Vector2 mouseOverChunkPos = World.GetChunkPosition(intersectedBlockPos.X, intersectedBlockPos.Z);

                if(game.World.loadedChunks.TryGetValue(mouseOverChunkPos, out Chunk cursorChunk))
                {
                    Vector3i mouseBlockLocalPos = intersectedBlockPos.ToChunkLocal();

                    lightDebug += (lightDebug != string.Empty ? " " : "") +
                                "Light at mouse R=" + cursorChunk.LightMap.GetRedBlockLightAt(mouseBlockLocalPos) +
                                " G=" + cursorChunk.LightMap.GetGreenBlockLightAt(mouseBlockLocalPos) +
                                " B=" + cursorChunk.LightMap.GetBlueBlockLightAt(mouseBlockLocalPos);

                    blockDebug = "Is Top Block=" + (currentChunk.TopMostBlocks[mouseBlockLocalPos.X, mouseBlockLocalPos.Z] == mouseBlockLocalPos.Y);
                }                      
            }

            if(lightDebug != string.Empty)
                sb.AppendLine(lightDebug);

            if(blockDebug != string.Empty)
                sb.AppendLine(blockDebug);

            sb.AppendLine("FPS=" + game.CurrentFPS + " AVG FPS=" + game.AverageFPSCounter.GetAverageFPS());
            sb.AppendLine("Block=" + game.ClientPlayer.mouseOverObject?.BlockstateHit?.ToString());
            sb.AppendLine("Time=" + game.World.Environment.CurrentTime);
            sb.AppendLine("IsServer=" + game.IsServer);

            debugText.Text = sb.ToString();
        }
    }
}
