using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Minecraft
{
    class Game
    {
        public static TextureLoader textureLoader;
        public static Input input;
        public static BlockDatabase blockDatabase;
        public static Random randomizer;

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
            randomizer = new Random();

            blockDatabase.RegisterBlocks();
            world = new World();
            world.GenerateTestMap();
            player = new Player(masterRenderer.projectionMatrix);
            input = new Input();

            //Thread t3 = new Thread(() => DoGenerateWorld());
            //t3.IsBackground = true;
            //t3.Start();
        }

        private List<Chunk> toProcessChunks = new List<Chunk>();

        private void DoGenerateWorld()
        {
            //Key already in dictionary and contains check while modifying dictionary!
            while (true)
            {
                int r = Constants.PLAYER_RENDER_DISTANCE;
                for (int x = -r; x <= r; x++)
                {
                    for (int z = -r; z <= r; z++)
                    {              
                        Vector2 chunkPos = world.GetChunkPosition(player.position.X, player.position.Z);
                        Vector2 toAttemptChunk = new Vector2(chunkPos.X + x, chunkPos.Y + z);

                        if (!world.chunks.ContainsKey(toAttemptChunk))
                        {
                            Chunk chunk = world.worldGenerator.GenerateBlocksForChunkAt((int)toAttemptChunk.X, (int)toAttemptChunk.Y);
                            toProcessChunks.Add(chunk);
                            Thread.Sleep(50);
                        }
                    }
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
                for(int i = toProcessChunks.Count - 1; i > 0; i--)
                {
                    Chunk toProcessChunk = toProcessChunks[i];
                    toProcessChunks.RemoveAt(i);
                    world.chunks.Add(new Vector2(toProcessChunk.gridX, toProcessChunk.gridZ), toProcessChunk);
                    world.chunkMeshGenerator.GenerateRenderMeshForChunk(toProcessChunk);
                }
            }

            world.EndFrameUpdate();
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
