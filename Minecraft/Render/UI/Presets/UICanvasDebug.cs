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
            float x = game.ClientPlayer.Position.X;
            float y = game.ClientPlayer.Position.Y;
            float z = game.ClientPlayer.Position.Z;

            Vector3i chunkLocalPos = new Vector3i(game.ClientPlayer.Position).ToChunkLocal();
            Vector2 chunkPos = World.GetChunkPosition(x, z);
            Vector3i gridPos = new Vector3i(game.ClientPlayer.Position);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Focused=" + game.Window.Focused + " Vsync=" + game.Window.VSync);

            sb.AppendLine("Position X=" + x.ToString("0.00") + " Y=" + y.ToString("0.00") + " Z=" + z.ToString("0.00") + 
                          " Grid Position X=" + gridPos.X + " Y=" + gridPos.Y + " Z= " + gridPos.Z);

            float vx = game.ClientPlayer.Velocity.X;
            float vy = game.ClientPlayer.Velocity.Y;
            float vz = game.ClientPlayer.Velocity.Z;
            sb.AppendLine("Velocity X=" + vx.ToString("0.00") + " Y=" + vy.ToString("0.00") + " Z=" + vz.ToString("0.00"));
            float ax = game.ClientPlayer.Acceleration.X;
            float ay = game.ClientPlayer.Acceleration.Y;
            float az = game.ClientPlayer.Velocity.Z;
            sb.AppendLine("Acceleration X=" + ax.ToString("0.00") + " Y=" + ay.ToString("0.00") + " Z=" + az.ToString("0.00"));
            
            sb.AppendLine("Chunk X=" + (int)chunkPos.X + " Z=" + (int)chunkPos.Y);

            if(game.World.loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                sb.AppendLine("Light sources in chunk=" + chunk.LightSourceBlocks.Count +
                              " Desired=" + game.MasterRenderer.DebugHelper.lightDebug.DesiredLightLevel +
                              " Debug=" + game.MasterRenderer.DebugHelper.renderBlockLightAreas);

                string lightDebug = string.Empty;
                if(chunkLocalPos.Y > 0 && chunkLocalPos.X < Constants.MAX_BUILD_HEIGHT)
                {
                    lightDebug += "Light at feet=" + chunk.LightMap.GetBlockLightAt(chunkLocalPos);
                }
                if(game.ClientPlayer.mouseOverObject != null)
                {
                    Vector3i intersectedBlockPos = game.ClientPlayer.mouseOverObject.IntersectedBlockPos;
                    Vector2 mouseOverChunkPos = World.GetChunkPosition(intersectedBlockPos.X, intersectedBlockPos.Z);
                    if(game.World.loadedChunks.TryGetValue(mouseOverChunkPos, out Chunk cursorChunk))
                    {
                        lightDebug += (lightDebug != string.Empty ? " " : "") + "Light at mouse=" + cursorChunk.LightMap.GetBlockLightAt(intersectedBlockPos.ToChunkLocal());
                    }                      
                }

                if(lightDebug != string.Empty)
                    sb.AppendLine(lightDebug);
            }

            sb.AppendLine("FPS=" + game.CurrentFPS + " AVG FPS=" + game.AverageFPSCounter.GetAverageFPS());
            sb.AppendLine("Block=" + game.ClientPlayer.mouseOverObject?.BlockstateHit?.ToString());
            sb.AppendLine("IsServer=" + game.IsServer);
            debugText.Text = sb.ToString();
        }
    }
}
