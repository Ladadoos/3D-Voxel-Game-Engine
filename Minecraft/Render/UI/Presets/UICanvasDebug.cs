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

            debugText = new UIText(this, game.MasterRenderer.GetFont(FontType.Arial), new Vector2(0, 0), new Vector2(0.4F, 0.4F), "");
            AddComponentToRender(debugText);
        }

        public override void Update()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Focused=" + game.Window.Focused + " Vsync=" + game.Window.VSync);
            float x = game.ClientPlayer.Position.X;
            float y = game.ClientPlayer.Position.Y;
            float z = game.ClientPlayer.Position.Z;
            sb.AppendLine("Position X=" + x.ToString("0.000") + " Y=" + y.ToString("0.000") + " Z=" + z.ToString("0.000"));
            float vx = game.ClientPlayer.Velocity.X;
            float vy = game.ClientPlayer.Velocity.Y;
            float vz = game.ClientPlayer.Velocity.Z;
            sb.AppendLine("Velocity X=" + vx.ToString("0.000") + " Y=" + vy.ToString("0.000") + " Z=" + vz.ToString("0.000"));
            float ax = game.ClientPlayer.Acceleration.X;
            float ay = game.ClientPlayer.Acceleration.Y;
            float az = game.ClientPlayer.Velocity.Z;
            sb.AppendLine("Acceleration X=" + ax.ToString("0.000") + " Y=" + ay.ToString("0.000") + " Z=" + az.ToString("0.000"));
            Vector2 chunkPos = World.GetChunkPosition(x, z);
            sb.AppendLine("Chunk X=" + (int)chunkPos.X + " Z=" + (int)chunkPos.Y);
            sb.AppendLine("FPS=" + game.CurrentFPS + " AVG FPS= " + game.AverageFPSCounter.GetAverageFPS());
            sb.AppendLine("Block=" + game.ClientPlayer.mouseOverObject?.BlockstateHit?.ToString());
            sb.AppendLine("IsServer=" + game.IsServer);
            debugText.Text = sb.ToString();
        }
    }
}
