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

namespace Minecraft.Main
{
    sealed class GameWindow : OpenTK.GameWindow
    {
        public MasterRenderer masterRenderer;
        public Loader loader;
        public Player player;
        public WorldMap world;
        public BlockDatabase blockDatabase;

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
            masterRenderer.Render(player.camera, world);
            SwapBuffers();
        }
    }
}