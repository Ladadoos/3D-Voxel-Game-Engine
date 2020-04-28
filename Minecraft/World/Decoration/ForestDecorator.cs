using System;

namespace Minecraft
{
    class ForestDecorator : IDecorator
    {
        private Random random = new Random();
        private OakTreeGenerator oakTreeGen = new OakTreeGenerator();

        public void Decorate(Chunk chunk, int worldY, int localX, int localZ)
        {
            if(random.Next(10) == 1)
            {
                chunk.AddBlock(localX, worldY, localZ, Blocks.GrassBlade.GetNewDefaultState());
            }else if(random.Next(300) == 1)
            {
                chunk.AddBlock(localX, worldY, localZ, Blocks.Flower.GetNewDefaultState());
            }else if(random.Next(50) == 1)
            {
                oakTreeGen.GenerateTreeAt(chunk, localX, worldY, localZ);
            }
        }
    }
}
