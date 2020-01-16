namespace Minecraft
{
    class SandBiome : Biome
    {
        public override void Decorate(Chunk chunk, int x, int y, int z)
        {
            if (Game.randomizer.Next(150) != 1)
            {
                return;
            }

            if (x > 2 && x < 13 && z > 2 && z < 13)
            {
                int r = 2 + Game.randomizer.Next(3);
                for (int yy = 1; yy < r; yy++)
                {
                    chunk.AddBlock(x, y + yy, z, Blocks.Stone.GetNewDefaultState());
                }
            }
        }
    }
}
