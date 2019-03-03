using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    class Game
    {
        public static TextureLoader textureLoader;
        public static Input input;
        public static BlockDatabase blockDatabase;

        public MasterRenderer masterRenderer;
        public Player player;
        public World world;

        private long totalElapsedFrames;
        private double totalElapsedTime;

        public void OnStartGame(GameWindow window)
        {
            textureLoader = new TextureLoader();
            masterRenderer = new MasterRenderer(window.Width, window.Height);
            blockDatabase = new BlockDatabase();
            blockDatabase.RegisterBlocks();
            world = new World();
            world.GenerateTestMap();
            player = new Player(masterRenderer.projectionMatrix);
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
            totalElapsedFrames++;
            totalElapsedTime += elapsedTime;

            input.Update();
            player.Update(world, (float)elapsedTime);
        }

        public void OnRenderGame()
        {
            masterRenderer.Render(player.camera, world);
        }

        public void OnWindowResize(int newWidth, int newHeight)
        {
            masterRenderer.width = newWidth;
            masterRenderer.height = newHeight;
            masterRenderer.CreateProjectionMatrix();
        }

        public int GetAverageFps()
        {
            return (int)(totalElapsedFrames / totalElapsedTime);
        }
    }
}
