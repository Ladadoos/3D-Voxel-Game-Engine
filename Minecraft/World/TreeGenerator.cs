using System;

namespace Minecraft
{
    class TreeGenerator
    {
        public Random random = new Random();

        public void GenerateTree(Chunk chunk, int x, int y, int z)
        {
            if (x > 3 && x < 12 && z > 3 && z < 12)
            {
                //Logger.log("    Tree generated", LogType.INFORMATION);
                int r = 2 + random.Next(3);
                for (int yy = 1; yy < r; yy++)
                {
                    chunk.AddBlock(x, y + yy, z, BlockType.Log);
                }
                y += r;
                x -= 2;
                z -= 2;
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        for (int k = 0; k < 2; k++)
                        {
                            chunk.AddBlock(x + i, y + k, z + j, BlockType.Leaves);
                        }
                    }
                }
                x += 2;
                z++;
                y += 2;
                chunk.AddBlock(x, y++, z, BlockType.Leaves);
                chunk.AddBlock(x--, y--, z++, BlockType.Leaves);
                for (int i = 0; i < 3; i++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        chunk.AddBlock(x + i, y + k, z, BlockType.Leaves);
                    }
                }
                x++;
                z++;
                chunk.AddBlock(x, y++, z, BlockType.Leaves);
                chunk.AddBlock(x, y, z, BlockType.Leaves);
            }
        }

    }
}
