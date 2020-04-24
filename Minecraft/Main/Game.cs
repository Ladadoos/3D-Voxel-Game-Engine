using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class Game
    {
        public GameWindow window { get; private set; }

        public static Input input { get; private set; }
    
        public MasterRenderer masterRenderer { get; private set; }
        public ClientPlayer player { get; private set; }
        public FPSCounter averageFpsCounter { get; private set; }
        public Client client { get; private set; }
        public WorldClient world { get; private set; }
        public Server server { get; private set; }
        public bool isServer { get; private set; }
        public RunMode mode { get; private set; }
        public float currentFps { get; private set; }

        private StartArgs startArgs;

        public Game(StartArgs startArgs)
        {
            this.mode = startArgs.runMode;
            this.startArgs = startArgs;

            isServer = mode == RunMode.ClientServer || mode == RunMode.Server;
        }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;
            window.VSync = OpenTK.VSyncMode.Off;
            window.CursorVisible = false;

            Blocks.RegisterBlocks();
            input = new Input();
            averageFpsCounter = new FPSCounter();

            if (mode == RunMode.ClientServer)
            {
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                server = new Server(this, true);
                server.Start(startArgs.ip, startArgs.port);

                world = new WorldClient(this);

                client = new Client(this);
                client.ConnectWith(startArgs.ip, startArgs.port);
            } else if(mode == RunMode.Server)
            {
                server = new Server(this, true);
                server.Start(startArgs.ip, startArgs.port);
            } else{
                player = new ClientPlayer(this);
                masterRenderer = new MasterRenderer(this);

                world = new WorldClient(this);

                client = new Client(this);
                client.ConnectWith(startArgs.ip, startArgs.port);
            }          
        }

        public void OnCloseGame()
        {
            if (mode != RunMode.Server)
            {
                masterRenderer.CleanUp();
            } else
            {

            }
        }

        public void OnUpdateGame(double deltaTime)
        {
            currentFps = (int)(1.0F / deltaTime);

            float elapsedSeconds = (float)deltaTime;
            elapsedSeconds = elapsedSeconds <= 0 ? 0.0001f : elapsedSeconds;

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
