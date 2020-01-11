using System;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class Game
    {
        public GameWindow window { get; private set; }

        public static Input input { get; private set; }
        public static Random randomizer { get; private set; }

        public MasterRenderer masterRenderer { get; private set; }
        public ClientPlayer player { get; private set; }
        public FPSCounter averageFpsCounter { get; private set; }
        public Client client { get; private set; }
        public World world { get; private set; }
        public Server server { get; private set; }
        public RunMode mode { get; private set; }
        public float currentFps;

        private StartArgs startArgs;

        public Game(StartArgs startArgs)
        {
            this.mode = startArgs.runMode;
            this.startArgs = startArgs;
        }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;

            Blocks.RegisterBlocks();
            randomizer = new Random();
            input = new Input();
            averageFpsCounter = new FPSCounter();

            if (mode == RunMode.ClientServer)
            {
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                server = new Server(this, false);
                server.Start(startArgs.ip, startArgs.port);
                WorldClient.AddHooks(this, server.world);
                server.GenerateMap();

                client = new Client(this);
                client.ConnectWith(startArgs.ip, startArgs.port);

                world = new WorldClient(this);
                world.loadedChunks = server.world.loadedChunks;
            } else if(mode == RunMode.Server)
            {
                server = new Server(this, true);
                server.Start(startArgs.ip, startArgs.port);
                server.GenerateMap();

                window.VSync = OpenTK.VSyncMode.On;
            } else{
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                world = new WorldClient(this);
                WorldClient.AddHooks(this, world);

                client = new Client(this);
                client.ConnectWith(startArgs.ip, startArgs.port);
            }          
        }

        public void OnCloseGame()
        {
            if (mode == RunMode.ClientServer)
            {
                masterRenderer.OnCloseGame();
            } else
            {

            }
        }

        public void OnUpdateGame(double deltaTime)
        {
            float elapsedSeconds = (float)deltaTime;

            averageFpsCounter.IncrementFrameCounter();
            averageFpsCounter.AddElapsedTime(deltaTime);

            if (mode == RunMode.Server)
            {
                server.world.Update(elapsedSeconds);
                server.Update();
            } else
            {
                if(mode == RunMode.ClientServer)
                {
                    server.world.Update(elapsedSeconds);
                    server.Update();
                }

                client.Update(elapsedSeconds);

                input.Update();
                player.Update(elapsedSeconds, world);

                world.Update(elapsedSeconds);

                masterRenderer.EndFrameUpdate(world);
            }
        }

        public void OnRenderGame()
        {
            if (mode != RunMode.Server)
            {
                masterRenderer.Render(world);
            } else
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
        }

        public void OnWindowResize(int newWidth, int newHeight)
        {
            if (mode != RunMode.Server)
            {
                player.camera.SetWindowSize(newWidth, newHeight);
            } else
            {

            }
        }
    }
}
