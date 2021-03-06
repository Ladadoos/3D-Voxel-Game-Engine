﻿namespace Minecraft
{
    class RockyDecorator : IDecorator
    {
        private readonly Noise3DPerlin perlin = new Noise3DPerlin(123123);
        private readonly OakTreeGenerator oakTreeGen = new OakTreeGenerator();

        public void Decorate(Chunk chunk, int worldY, int localX, int localZ)
        {
            int worldX = chunk.GridX * 16 + localX;
            int worldZ = chunk.GridZ * 16 + localZ;

            double perl = perlin.GetValue(worldX * 0.0075f, worldY * 0.0075f, worldZ * 0.0075f);
            if(perl < -0.75f)
            {
                chunk.AddBlockAt(localX, worldY - 1, localZ, Blocks.GetState(Blocks.Gravel));
            }else if(perl < -0.45f)
            {
                for(int i = 1; i <= 3; i++)
                {
                    chunk.AddBlockAt(localX, worldY - i, localZ, Blocks.GetState(Blocks.Dirt));
                }
     
                double foliagePerl = perlin.GetValue(worldX * 0.75f, worldY * 0.75f, worldZ * 0.75f);
                if(foliagePerl < -0.9D)
                {
                    oakTreeGen.GenerateTreeAt(chunk, localX, worldY, localZ); oakTreeGen.GenerateTreeAt(chunk, localX, worldY, localZ);
                }else if(foliagePerl < -0.5D)
                {
                    chunk.AddBlockAt(localX, worldY, localZ, Blocks.GetState(Blocks.GrassBlade));
                }
            }
        }
    }
}
