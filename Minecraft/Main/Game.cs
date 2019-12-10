using System;

namespace Minecraft
{
    class Game
    {
        public GameWindow window { get; private set; }

        public static TextureLoader textureLoader { get; private set; }
        public static Input input { get; private set; }
        public static BlockDatabase blockDatabase { get; private set; }
        public static Random randomizer { get; private set; }

        public MasterRenderer masterRenderer { get; private set; }
        public Player player { get; private set; }
        public World world { get; private set; }
        public FPSCounter fpsCounter { get; private set; }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;

            player = new Player(this);

            fpsCounter = new FPSCounter();
            textureLoader = new TextureLoader();
            masterRenderer = new MasterRenderer(this);
            blockDatabase = new BlockDatabase();
            randomizer = new Random();

            blockDatabase.RegisterBlocks();
            world = new World(this);
            world.GenerateTestMap();
            input = new Input();
        }

        public void OnCloseGame()
        {
            masterRenderer.CleanUp();
            textureLoader.CleanUp();
            world.CleanUp();
        }

        public void OnUpdateGame(double elapsedTime)
        {
            fpsCounter.IncrementFrameCounter();
            fpsCounter.AddElapsedTime(elapsedTime);

            input.Update();
            player.Update((float)elapsedTime);

            world.EndFrameUpdate();
        }

        public void OnRenderGame()
        {
            masterRenderer.Render(world);
        }

        public void OnWindowResize(int newWidth, int newHeight)
        {
            player.camera.SetWindowSize(newWidth, newHeight);
        }
    }
}
