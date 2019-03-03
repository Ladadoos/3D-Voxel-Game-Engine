using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

            Thread t3 = new Thread(() => DoGenerateWorld());
            t3.IsBackground = true;
            t3.Start();
        }

        private Queue<Chunk> toProcessChunks = new Queue<Chunk>();

        private void DoGenerateWorld()
        {
            while (true)
            {
                Thread.Sleep(100);
                Vector2 chunkPos = world.GetChunkPosition(player.position.X, player.position.Z);
                if (!world.chunks.ContainsKey(chunkPos))
                {
                    Chunk chunk = world.GenerateBlocksForChunk((int)chunkPos.X, (int)chunkPos.Y);
                    toProcessChunks.Enqueue(chunk);
                }
            }
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

            if(toProcessChunks.Count > 0)
            {
                for(int i = 0; i < toProcessChunks.Count; i++)
                {
                    Chunk toProcessChunk = toProcessChunks.Dequeue();
                    world.chunkMeshGenerator.PrepareChunkToRender(toProcessChunk, true);
                }
            }
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
