using System;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Minecraft
{

    //player.position is not center of player!

    sealed class GameWindow : OpenTK.GameWindow
    {
        public MasterRenderer masterRenderer;
        public Loader loader;
        public Player player;
        public World world;
        public BlockDatabase blockDatabase;
        public Input input;



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
            world = new World(blockDatabase);
            world.GenerateTestMap();
            player = new Player(masterRenderer.projectionMatrix);
            input = new Input();
        }

        private long elapsedFrames;
        private double elapsedTime;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            elapsedFrames++;
            elapsedTime += e.Time;
            Vector2 chunkPos = world.GetChunkPosition(player.position.X, player.position.Z);
            Title = "Vsync: " + VSync + " FPS: " + (int)(1f / e.Time) + " AVG FPS: " + (int)(elapsedFrames / elapsedTime) + " Position: " + player.position + "Grid Pos: " + chunkPos;
            if(OpenTK.Input.Keyboard.GetState().IsKeyDown(Key.Escape)) {
                Exit();
            }
            input.Update();
            player.Update(this, world, (float)e.Time, input);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        { 
            masterRenderer.Render(player.camera, world);
            SwapBuffers();
        }
    }
}