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
        public FPSCounter fpsCounter { get; private set; }
        public Client client { get; private set; }
        public World world { get; private set; }
        public Server server { get; private set; }
        public RunMode mode { get; private set; }

        public Game(RunMode mode)
        {
            this.mode = mode;
        }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;
            Logger.SetLogLevel(LogLevel.Info);

            Blocks.RegisterBlocks();
            randomizer = new Random();
            input = new Input();
            fpsCounter = new FPSCounter();
            window.VSync = OpenTK.VSyncMode.On;

            if (mode == RunMode.ClientServer)
            {
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                server = new Server(this, true);
                server.Start("127.0.0.1", 50000);
                WorldClient.AddHooks(this, server.world);
                server.GenerateMap();

                client = new Client(this);
                client.ConnectWith("127.0.0.1", 50000);

                world = new WorldClient(this);
                world.loadedChunks = server.GetChunKStorage();
            } else if(mode == RunMode.Server)
            {
                server = new Server(this, true);
                server.Start("127.0.0.1", 50000);
                server.GenerateMap();

                window.VSync = OpenTK.VSyncMode.On;
            } else{
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                world = new WorldClient(this);
                WorldClient.AddHooks(this, world);

                client = new Client(this);
                client.ConnectWith("127.0.0.1", 50000);
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

            fpsCounter.IncrementFrameCounter();
            fpsCounter.AddElapsedTime(deltaTime);

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
