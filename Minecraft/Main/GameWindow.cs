using System;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Minecraft
{
    sealed class GameWindow : OpenTK.GameWindow
    {
        private Game game;

        public GameWindow(RunMode mode) : base(720, 480, GraphicsMode.Default, "Minecraft OpenGL", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Logger.Info("OpenGL version: " + GL.GetString(StringName.Version));
            game = new Game(mode);

            if(mode == RunMode.Server)
            {
                Width = 10;
                Height = 10;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            game.OnCloseGame();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);

            game.OnWindowResize(Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            VSync = VSyncMode.On;

            game.OnStartGame(this);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Game.input.OnKeyPress(Key.Escape))
            {
                Exit();
            }

            if(game.mode != RunMode.Server)
            {
                Vector2 chunkPos = game.world.GetChunkPosition(game.player.position.X, game.player.position.Z);
                Title = "Focused:" + Focused + 
                        "Vsync: " + VSync +
                        " FPS: " + (int)(1f / e.Time) +
                        " AVG FPS: " + game.fpsCounter.GetAverageFPS() +
                        " Position: " + game.player.position +
                        " Grid Pos: " + chunkPos +
                        " Velocity: " + game.player.velocity;
            }


            game.OnUpdateGame(e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            game.OnRenderGame();
            SwapBuffers();
        }
    }
}