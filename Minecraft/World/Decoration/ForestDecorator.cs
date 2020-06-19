using System;

namespace Minecraft
{
    class ForestDecorator : IDecorator
    {
        private readonly Random random = new Random();
        private readonly OakTreeGenerator oakTreeGen = new OakTreeGenerator();

        public void Decorate(Chunk chunk, int worldY, int localX, int localZ)
        {
            if(random.Next(10) == 1)
            {
                chunk.AddBlockAt(localX, worldY, localZ, Blocks.GetState(Blocks.GrassBlade));
            }else if(random.Next(300) == 1)
            {
                chunk.AddBlockAt(localX, worldY, localZ, Blocks.GetState(Blocks.Flower));
            }else if(random.Next(50) == 1)
            {
                oakTreeGen.GenerateTreeAt(chunk, localX, worldY, localZ);
            }
        }
    }
}
