using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class Game
    {
        public GameWindow Window { get; private set; }

        public static Input Input { get; private set; }
    
        public MasterRenderer MasterRenderer { get; private set; }
        public ClientPlayer ClientPlayer { get; private set; }
        public FPSCounter AverageFPSCounter { get; private set; }
        public Client Client { get; private set; }
        public WorldClient World { get; private set; }
        public Server Server { get; private set; }
        public bool IsServer { get; private set; }
        public RunMode RunMode { get; private set; }
        public float CurrentFPS { get; private set; }

        private StartArgs startArgs;

        public Game(StartArgs startArgs)
        {
            this.RunMode = startArgs.RunMode;
            this.startArgs = startArgs;

            IsServer = RunMode == RunMode.ClientServer || RunMode == RunMode.Server;
        }

        public void OnStartGame(GameWindow window)
        {
            this.Window = window;
            window.VSync = OpenTK.VSyncMode.On;
            window.CursorVisible = false;

            Blocks.RegisterBlocks();
            FontRegistry.Initialize();

            Input = new Input();
            AverageFPSCounter = new FPSCounter();

            if (RunMode == RunMode.ClientServer)
            {
                ClientPlayer = new ClientPlayer(this);
                MasterRenderer = new MasterRenderer(this);

                Server = new Server(this, true);
                Server.Start(startArgs.IP, startArgs.Port);

                World = new WorldClient(this);

                Client = new Client(this);
                Client.ConnectWith(startArgs.IP, startArgs.Port);
            } else if(RunMode == RunMode.Server)
            {
                Server = new Server(this, true);
                Server.Start(startArgs.IP, startArgs.Port);
            } else{
                ClientPlayer = new ClientPlayer(this);
                MasterRenderer = new MasterRenderer(this);

                World = new WorldClient(this);

                Client = new Client(this);
                Client.ConnectWith(startArgs.IP, startArgs.Port);
            }          
        }

        public void OnCloseGame()
        {
            if (RunMode != RunMode.Server)
            {
                MasterRenderer.CleanUp();
            }
        }

        public void OnUpdateGame(double deltaTime)
        {
            CurrentFPS = (int)(1.0F / deltaTime);

            float elapsedSeconds = (float)deltaTime;
            elapsedSeconds = elapsedSeconds <= 0 ? 0.0001f : elapsedSeconds;

            AverageFPSCounter.IncrementFrameCounter();
            AverageFPSCounter.AddElapsedTime(deltaTime);

            if (RunMode == RunMode.Server)
            {
                Server.World.Update(elapsedSeconds);
                Server.Update();
            } else
            {
                if(RunMode == RunMode.ClientServer)
                {
                    Server.World.Update(elapsedSeconds);
                    Server.Update();
                }
                Client.Update(elapsedSeconds);
                Input.Update();
                World.Update(elapsedSeconds);
                MasterRenderer.EndFrameUpdate(World);
            }
        }

        public void OnRenderGame()
        {
            if (RunMode != RunMode.Server)
            {
                MasterRenderer.Render(World);
            } else
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
        }

        public void OnWindowResize(int newWidth, int newHeight)
        {
            if (RunMode != RunMode.Server)
            {
                ClientPlayer.camera.SetWindowSize(newWidth, newHeight);
            }
        }
    }
}
