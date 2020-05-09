using OpenTK;
using System.Text;

namespace Minecraft
{
    class UICanvasDebug : UICanvas
    {
        private Game game;
        private UIText debugText;

        public UICanvasDebug(Game game) : 
            base(Vector3.Zero, Vector3.Zero, game.window.Width, game.window.Height, RenderSpace.Screen)
        {
            this.game = game;

            debugText = new UIText(this, game.masterRenderer.GetFont(FontType.Arial), new Vector2(0, 0), new Vector2(0.4F, 0.4F), "");
            AddComponentToRender(debugText);
        }

        public override void Update()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Focused=" + game.window.Focused + " Vsync=" + game.window.VSync);
            float x = game.player.position.X;
            float y = game.player.position.Y;
            float z = game.player.position.Z;
            sb.AppendLine("Position X=" + x.ToString("0.000") + " Y=" + y.ToString("0.000") + " Z=" + z.ToString("0.000"));
            float vx = game.player.velocity.X;
            float vy = game.player.velocity.Y;
            float vz = game.player.velocity.Z;
            sb.AppendLine("Velocity X=" + vx.ToString("0.000") + " Y=" + vy.ToString("0.000") + " Z=" + vz.ToString("0.000"));
            float ax = game.player.acceleration.X;
            float ay = game.player.acceleration.Y;
            float az = game.player.velocity.Z;
            sb.AppendLine("Acceleration X=" + ax.ToString("0.000") + " Y=" + ay.ToString("0.000") + " Z=" + az.ToString("0.000"));
            Vector2 chunkPos = World.GetChunkPosition(x, z);
            sb.AppendLine("Chunk X=" + (int)chunkPos.X + " Z=" + (int)chunkPos.Y);
            sb.AppendLine("FPS=" + game.currentFps + " AVG FPS= " + game.averageFpsCounter.GetAverageFPS());
            sb.AppendLine("Block=" + game.player.mouseOverObject?.blockstateHit?.ToString());
            sb.AppendLine("IsServer=" + game.isServer);
            debugText.text = sb.ToString();
        }
    }
}
