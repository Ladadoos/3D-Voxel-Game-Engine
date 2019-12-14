using System;

namespace Minecraft
{
    class Game
    {
        public GameWindow window { get; private set; }

        public static Input input { get; private set; }
        public static Random randomizer { get; private set; }

        public MasterRenderer masterRenderer { get; private set; }
        public Player player { get; private set; }
        public World world { get; private set; }
        public FPSCounter fpsCounter { get; private set; }

        public void OnStartGame(GameWindow window)
        {
            this.window = window;

            randomizer = new Random();
            input = new Input();
            player = new Player(this);
            fpsCounter = new FPSCounter();
            masterRenderer = new MasterRenderer(this);          
            world = new World(this);
            world.GenerateTestMap(masterRenderer);
        }

        public void OnCloseGame()
        {
            masterRenderer.OnCloseGame();
        }

        public void OnUpdateGame(double elapsedTime)
        {
            fpsCounter.IncrementFrameCounter();
            fpsCounter.AddElapsedTime(elapsedTime);

            input.Update();
            player.Update((float)elapsedTime);

            masterRenderer.EndFrameUpdate(world);
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
