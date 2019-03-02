using System;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using Minecraft.Tools;
using Minecraft.Entities;
using Minecraft.Render;
using Minecraft.World;
using Minecraft.World.Blocks;
using System.Drawing;

namespace Minecraft.Main
{
    sealed class GameWindow : OpenTK.GameWindow
    {
        public MasterRenderer masterRenderer;
        public Loader loader;
        public Player player;
        public WorldMap world;
        public BlockDatabase blockDatabase;


        public TextRenderer renderer;
        Font serif = new Font(FontFamily.GenericSerif, 24);
        Font sans = new Font(FontFamily.GenericSansSerif, 24);
        Font mono = new Font(FontFamily.GenericMonospace, 24);

        public GameWindow() : base(1920, 1080, GraphicsMode.Default, "Minecraft OpenGL", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, GraphicsContextFlags.ForwardCompatible) {
            Logger.Log("OpenGL version: " + GL.GetString(StringName.Version), LogType.INFORMATION);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            masterRenderer.CleanUp();
            loader.CleanUp();
            world.CleanUp();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = false;
            VSync = VSyncMode.Off;

            loader = new Loader();
            masterRenderer = new MasterRenderer(Width, Height);
            blockDatabase = new BlockDatabase(loader);
            blockDatabase.RegisterBlocks();
            world = new WorldMap(blockDatabase);
            world.GenerateTestMap();
            player = new Player(masterRenderer.projectionMatrix);

            /*renderer = new TextRenderer(Width, Height);
            PointF position = PointF.Empty;

          
            renderer.Clear(Color.Red);
            renderer.DrawString("The quick brown fox jumps over the lazy dog", serif, Brushes.White, position);
            position.Y += serif.Height;
            renderer.DrawString("The quick brown fox jumps over the lazy dog", sans, Brushes.White, position);
            position.Y += sans.Height;
            renderer.DrawString("The quick brown fox jumps over the lazy dog", mono, Brushes.White, position);
            position.Y += mono.Height;*/
        }

        private long elapsedFrames;
        private double elapsedTime;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            elapsedFrames++;
            elapsedTime += e.Time;
            Title = "Vsync: " + VSync + " FPS: " + (int)(1f / e.Time) + " AVG FPS: " + (int)(elapsedFrames / elapsedTime) + " Position: " + player.position;
            if(OpenTK.Input.Keyboard.GetState().IsKeyDown(Key.Escape)) {
                Exit();
            }
            player.Update(this, world, (float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            /*GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, renderer.Texture);
            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1f, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1f, 1f);

            GL.End();*/

            masterRenderer.Render(player.camera, world);
            SwapBuffers();
        }
    }
}