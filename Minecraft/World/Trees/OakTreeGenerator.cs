using System;

namespace Minecraft
{
    class OakTreeGenerator : ITreeGenerator
    {
        private readonly Random random = new Random();

        public void GenerateTreeAt(Chunk chunk, int localX, int worldY, int localZ)
        {
            if (localX > 2 && localX < 13 && localZ > 2 && localZ < 13)
            {
                int trunckX = localX;
                int trunckZ = localZ;
                int r = 2 + random.Next(3);
                for (int yy = 0; yy < r + 4; yy++)
                {
                    chunk.AddBlockAt(localX, worldY + yy, localZ, Blocks.OakLog.GetNewDefaultState());
                }
                worldY += r;
                localX -= 2;
                localZ -= 2;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if(localX + i == trunckX && localZ + j == trunckZ)
                        {
                            continue;
                        }
                        for (int k = 0; k < 2; k++)
                        {
                            chunk.AddBlockAt(localX + i, worldY + k, localZ + j, Blocks.OakLeaves.GetNewDefaultState());
                        }
                    }
                }
                localX += 2;
                localZ++;
                worldY += 2;
                chunk.AddBlockAt(localX, worldY++, localZ, Blocks.OakLeaves.GetNewDefaultState());
                chunk.AddBlockAt(localX--, worldY--, localZ++, Blocks.OakLeaves.GetNewDefaultState());
                for (int i = 0; i < 3; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        chunk.AddBlockAt(localX + i, worldY + k, localZ, Blocks.OakLeaves.GetNewDefaultState());
                    }
                }
                localX++;
                localZ++;
                chunk.AddBlockAt(localX, worldY++, localZ, Blocks.OakLeaves.GetNewDefaultState());
                chunk.AddBlockAt(localX, worldY, localZ, Blocks.OakLeaves.GetNewDefaultState());
            }
        }

    }
}
