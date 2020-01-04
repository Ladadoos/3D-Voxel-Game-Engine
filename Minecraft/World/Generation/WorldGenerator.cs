using LibNoise;

namespace Minecraft
{
    class WorldGenerator
    {
        private Perlin basePerlinFunction = new Perlin();
        private double basePerlinDetail = 0.006D; //Smaller means more detail
        private int basePerlinSeed;

        private Perlin biomePerlinFunction = new Perlin();
        private double biomePerlinDetail = 0.0045D; //Smaller means larger biomes
        private int biomePerlinSeed;

        private RockyBiome rockBiome = new RockyBiome();
        private ForestBiome forestBiome = new ForestBiome();
        private SandBiome sandBiome = new SandBiome();

        private int seaLevel = 95;

        public WorldGenerator()
        {
            basePerlinSeed = Game.randomizer.Next(1000000);
            biomePerlinSeed = Game.randomizer.Next(1000000);
        }

        private double GetBasePerlinValueAt(double x, double y)
        {
            return basePerlinFunction.GetValue(x + basePerlinSeed, 1, y + basePerlinSeed);
        }

        private double GetBiomePerlinValueAt(double x, double y)
        {
            return biomePerlinFunction.GetValue(x + biomePerlinSeed, 1, y + biomePerlinSeed);
        }

        public Chunk GenerateBlocksForChunkAt(int x, int y)
        {
            Chunk generatedChunk = new Chunk(x, y);

            double baseXoffset = 0;
            double baseYOffset = 0;
            double biomeXoffset = 0;
            double biomeYOffset = 0;

            baseYOffset = x * Constants.CHUNK_SIZE * basePerlinDetail;
            biomeYOffset = x * Constants.CHUNK_SIZE * biomePerlinDetail;

            for (int i = 0; i < Constants.CHUNK_SIZE; i++)
            {
                baseXoffset = y * Constants.CHUNK_SIZE * basePerlinDetail;
                biomeXoffset = y * Constants.CHUNK_SIZE * biomePerlinDetail;

                for (int j = 0; j < Constants.CHUNK_SIZE; j++)
                {
                    double basePerlinValue = GetBasePerlinValueAt(baseXoffset, baseYOffset);
                    int height = seaLevel + System.Math.Abs((int)(basePerlinValue * 32));

                    double biomeDeterminer = GetBiomePerlinValueAt(biomeXoffset, biomeYOffset);
                    if (biomeDeterminer > 0.75D)
                    {
                        if (Game.randomizer.Next(25) != 1)
                        {
                            generatedChunk.AddBlock(i, height, j, Blocks.Dirt.GetNewDefaultState());
                        } else
                        {
                            generatedChunk.AddBlock(i, height, j, Blocks.Dirt.GetNewDefaultState());
                        }
                    } else if (biomeDeterminer < -0.75D)
                    {
                        generatedChunk.AddBlock(i, height, j, Blocks.Dirt.GetNewDefaultState());
                    } else if (biomeDeterminer > -0.75D && biomeDeterminer < 0.25D)
                    {
                        forestBiome.Decorate(generatedChunk, i, height, j);
                        generatedChunk.AddBlock(i, height, j, Blocks.Dirt.GetNewDefaultState());
                    } else if (biomeDeterminer > 0.25D && biomeDeterminer < 0.75D)
                    {
                        sandBiome.Decorate(generatedChunk, i, height, j);
                        generatedChunk.AddBlock(i, height, j, Blocks.Dirt.GetNewDefaultState());
                    }

                    int k = height - 1;
                    while (k >= 0)
                    {
                        int r = Game.randomizer.Next(1000);
                        if (r == 1)
                        {
                            generatedChunk.AddBlock(i * 1, k, j * 1, Blocks.Flower.GetNewDefaultState());
                        } else
                        {
                            generatedChunk.AddBlock(i * 1, k, j * 1, Blocks.Stone.GetNewDefaultState());
                        }

                        k--;
                    }

                    baseXoffset += basePerlinDetail;
                    biomeXoffset += biomePerlinDetail;
                }
                baseYOffset += basePerlinDetail;
                biomeYOffset += biomePerlinDetail;
            }
            return generatedChunk;
        }
    }
}
