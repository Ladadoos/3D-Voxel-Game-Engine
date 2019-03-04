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
        public static GameWindow instance;

        private Game game;

        public GameWindow() : base(1920, 1080, GraphicsMode.Default, "Minecraft OpenGL", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Logger.Log("OpenGL version: " + GL.GetString(StringName.Version), LogType.INFORMATION);
            game = new Game();
            instance = this;
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
            VSync = VSyncMode.Off;

            game.OnStartGame(this);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if(Game.input.OnKeyPress(Key.Escape)) {
                Exit();
            }

            Vector2 chunkPos = game.world.GetChunkPosition(game.player.position.X, game.player.position.Z);
            Title = "Vsync: " + VSync + 
                    " FPS: " + (int)(1f / e.Time) + 
                    " AVG FPS: " + game.GetAverageFps() + 
                    " Position: " + game.player.position + 
                    " Grid Pos: " + chunkPos;

            game.OnUpdateGame(e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            game.OnRenderGame();
            SwapBuffers();
        }
    }
}