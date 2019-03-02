using System;
using System.Collections.Generic;

using OpenTK;

using LibNoise;
using Minecraft.World.Blocks;
using Minecraft.World.Chunks;

namespace Minecraft.World
{
    class WorldMap
    {
        public const int MaxRenderDistance = 32;

        public int seed;

        public Random random = new Random();

        public static Perlin perlin = new Perlin();
        public double perlinNoise = 0.0025D; //0.0006f

        public TreeGenerator treeGenerator;
        public ChunkMeshGenerator chunkMeshGenerator;
        public Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();

        public BlockDatabase db;

        public WorldMap(BlockDatabase db)
        {
            chunkMeshGenerator = new ChunkMeshGenerator(db, this);
            treeGenerator = new TreeGenerator();

            seed = random.Next(10000);

            this.db = db;
        }

        public void CleanUp()
        {
            foreach (KeyValuePair<Vector2, Chunk> gridChunk in chunks)
            {
                gridChunk.Value.model.CleanUp();
            }
        }

        public void GenerateTestMap()
        {
            for (int x = 0; x < 15; x += 2)
            {
                for (int y = 0; y < 15; y += 2)
                {
                    GenerateBlocksForChunk(x, y);
                }
            }
            Console.WriteLine("Generation completed.");
        }


        public void GenerateBlocksForChunk(int x, int y)
        {
  


            /*double xOff = 0;
            double yOff = 0;
            Chunk chunkC;

            yOff = x * Constants.CHUNK_SIZE * perlinNoise;
            chunkC = new Chunk(x, y);
            for (int i = 0; i < Constants.CHUNK_SIZE; i++)
            {
                xOff = y * Constants.CHUNK_SIZE * perlinNoise;
                for (int j = 0; j < Constants.CHUNK_SIZE; j++)
                {
                    int height = 128 + System.Math.Abs((int)(perlin.GetValue(xOff + seed, 1, yOff + seed) * 75));
                    //int height = 10;
                    if (height < 136)
                    {
                        chunkC.AddBlock(i * 1, height * 1, j * 1, BlockType.Sand);
                    }
                    else
                    {
                        chunkC.AddBlock(i * 1, height * 1, j * 1, BlockType.Grass);
                        int r = random.Next(150);
                        if (r == 1)
                        {
                            treeGenerator.GenerateTree(chunkC, i, height, j);
                        }
                    }
                    int k = height - 1;
                    while (k >= 0)
                    {
                        int r = random.Next(1000);
                        if (r < 10)
                        {
                            chunkC.AddBlock(i * 1, k, j * 1, BlockType.Redstone_Ore);
                        }
                        else
                        {
                            chunkC.AddBlock(i * 1, k, j * 1, BlockType.Stone);
                        }

                        k--;
                    }
                    xOff += perlinNoise;
                }
                yOff += perlinNoise;
            }*/

            Chunk chunkC = new Chunk(x, y);
            int yyy = Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT - 1;
            Array val = Enum.GetValues(typeof(BlockType));
            for (int i = 0; i < Constants.CHUNK_SIZE; i++)
            {
                for (int j = 0; j < Constants.CHUNK_SIZE; j++)
                {
                    for (int k = 0; k < yyy; k++)
                    {
                        // sbyte r = (sbyte)(random.Next(10) + 1);
                        chunkC.AddBlock(i, k, j, (BlockType)(val.GetValue(random.Next(10) + 1)));
                    }
                }
            }
            //chunks.Add(new Vector2(x, y), chunkC);
            //chunks[x, y] = chunkC;
            chunks.Add(new Vector2(x, y), chunkC);
            var start = DateTime.Now;
            chunkMeshGenerator.PrepareChunkToRender(chunkC);
            var now2 = DateTime.Now - start;
            Console.WriteLine("Chunk time: " + now2);
            //Logger.log("Block gen time[" + now + "]     Mesh updating time[" + now2 + "]     Chunk count[" + chunks.Count + "]", LogType.INFORMATION);
        }

        public Vector2 GetChunkPosition(float xPos, float yPos)
        {
            int x = (int)(System.Math.Ceiling(xPos / Constants.CHUNK_SIZE)) - 1;
            int z = (int)(System.Math.Ceiling(yPos / Constants.CHUNK_SIZE)) - 1;
            return new Vector2(x, z);
        }
    }
}
