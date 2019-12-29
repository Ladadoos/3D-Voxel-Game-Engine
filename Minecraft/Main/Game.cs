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
        public ClientPlayer player;// { get; private set; }
        public FPSCounter fpsCounter { get; private set; }
        public Client client { get; private set; }
        public World world { get; private set; }
        public Server localServer { get; private set; }
        public RunMode mode { get; private set; }
        public bool isReadyToPlay = false;
        private bool initialized = false;

        public Game(RunMode mode)
        {
            this.mode = mode;
        }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;

            Blocks.RegisterBlocks();
            randomizer = new Random();
            input = new Input();
            fpsCounter = new FPSCounter();

            if (mode == RunMode.ClientServer)
            {
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                localServer = new Server();
                localServer.Start(this, "127.0.0.1", 50000);
                localServer.AddHook(new ClientWorldHook(this));
                //localServer.AddHook(new ServerWorldHook(this));
                localServer.GenerateMap();

                client = new Client(this);
                client.ConnectWith("127.0.0.1", 50000);

                world = localServer.GetWorldInstance();
            } else if(mode == RunMode.Server)
            {
                localServer = new Server();
                localServer.Start(this, "127.0.0.1", 50000);
                localServer.AddHook(new ServerWorldHook(this));
                localServer.GenerateMap();

                world = localServer.GetWorldInstance();

                window.VSync = OpenTK.VSyncMode.On;
            } else{
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                world = new World(this);
                world.AddEventHooks(new ClientWorldHook(this));

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

        public void OnUpdateGame(double elapsedSeconds)
        {
            fpsCounter.IncrementFrameCounter();
            fpsCounter.AddElapsedTime(elapsedSeconds);

            if (mode == RunMode.ClientServer)
            {
                localServer?.Update(this);
                client?.Update(this);

                input.Update();
                player.Update((float)elapsedSeconds, world);

                world.Tick((float)elapsedSeconds);

                masterRenderer.EndFrameUpdate(world);
            } else if(mode == RunMode.Client)
            {
                client?.Update(this);

                input.Update();
                player.Update((float)elapsedSeconds, world);

                world.Tick((float)elapsedSeconds);

                masterRenderer.EndFrameUpdate(world);
            } else
            {
                world.Tick((float)elapsedSeconds);

                localServer.Update(this);
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
