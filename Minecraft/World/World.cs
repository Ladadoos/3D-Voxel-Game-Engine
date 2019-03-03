using System;
using System.Collections.Generic;

using OpenTK;

using LibNoise;

namespace Minecraft
{
    class World
    {
        public const int MaxRenderDistance = 32;

        public int seed;

        public Random random = new Random();

        public static Perlin perlin = new Perlin();
        public double perlinNoise = 0.002D; //0.0006f

        public TreeGenerator treeGenerator;
        public ChunkMeshGenerator chunkMeshGenerator;
        public Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();
        public Dictionary<Vector2, RenderChunk> renderChunks = new Dictionary<Vector2, RenderChunk>();

        public World()
        {
            chunkMeshGenerator = new ChunkMeshGenerator(this);
            treeGenerator = new TreeGenerator();

            seed = random.Next(10000);
        }

        public void CleanUp()
        {
            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in renderChunks)
            {
                chunkToRender.Value.OnApplicationClosed();
            }
        }

        public void GenerateTestMap()
        {
            var start = DateTime.Now;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                   Chunk chunk = GenerateBlocksForChunk(x, y);
                   chunkMeshGenerator.PrepareChunkToRender(chunk, true);
                }
            }
            var now2 = DateTime.Now - start;
            Console.WriteLine("Generating init chunks took: " + now2 + " s");
        }

        public Chunk GenerateBlocksForChunk(int x, int y)
        {
            double xOff = 0;
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
            }

            /*Chunk chunkC = new Chunk(x, y);
            int yyy = random.Next(100);//Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT - 1;
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
            }*/
            //chunks.Add(new Vector2(x, y), chunkC);
            //chunks[x, y] = chunkC;
            chunks.Add(new Vector2(x, y), chunkC);
            //var start = DateTime.Now;
            Console.WriteLine("Generated chunk " + x + "," + y);
            return chunkC;
            //chunkMeshGenerator.PrepareChunkToRender(chunkC, true);

            //Logger.log("Block gen time[" + now + "]     Mesh updating time[" + now2 + "]     Chunk count[" + chunks.Count + "]", LogType.INFORMATION);
        }

        public Vector2 GetChunkPosition(float xPos, float yPos)
        {
            //int x = (int)(System.Math.Ceiling(xPos / Constants.CHUNK_SIZE)) - 1;
            //int z = (int)(System.Math.Ceiling(yPos / Constants.CHUNK_SIZE)) - 1;
            return new Vector2((int)xPos >> 4, (int)yPos >> 4);
        }

        public bool AddBlockToWorld(int x, int y, int z, BlockType blockType)
        {
            if (IsOutsideBuildHeight(y))
            {
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(x, z);
            Chunk chunk;
            if(!chunks.TryGetValue(chunkPos, out chunk))
            {
                Console.WriteLine("tried to add block in junk that doesnt exist");
                return false;
            }

            int i = x & 15;
            int j = y;
            int k = z & 15;

            chunk.AddBlock(i, j, k, blockType);

            bool updateSurroundingChunks = i == 0 || i == 15 || k == 0 || k == 15;
            chunkMeshGenerator.PrepareChunkToRender(chunk, updateSurroundingChunks);
            Console.WriteLine("Tried to add block in chunk" + i + "," + j + "," + k);
            return true;
        }

        public bool IsOutsideBuildHeight(int height)
        {
            return height < 0 || height >= Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT;
        }
        
        public BlockType GetBlockAt(int x, int y, int z)
        {
            if (IsOutsideBuildHeight(y))
            {
                return BlockType.Air;
            }

            Vector2 chunkPos = GetChunkPosition(x, z);
            Chunk chunk;
            if (!chunks.TryGetValue(chunkPos, out chunk))
            {
                return BlockType.Air;
            }

            int sectionHeight = y / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return BlockType.Air;
            }

            int localX = x & 15;    
            int localY = y & 15;
            int localZ = z & 15;

            sbyte? blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return BlockType.Air;
            }
            return (BlockType)blockType;
        }
    }
}
