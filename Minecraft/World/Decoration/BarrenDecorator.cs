using System;

namespace Minecraft
{
    class BarrenDecorator : IDecorator
    {
        private readonly Random random = new Random();

        public void Decorate(Chunk chunk, int worldY, int localX, int localZ)
        {
            if(random.Next(300) == 1)
            {
                int cactusHeight = 2 + random.Next(3);
                for(int i = worldY; i < worldY + cactusHeight; i++)
                {
                    chunk.AddBlock(localX, i, localZ, Blocks.Cactus.GetNewDefaultState());
                }
            }else if(random.Next(200) == 1)
            {
                chunk.AddBlock(localX, worldY, localZ, Blocks.DeadBush.GetNewDefaultState());
            }
        }
    }
}
