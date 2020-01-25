namespace Minecraft
{
    class WorldGenerator
    {
        private MountainBiome mountainBiome = new MountainBiome();
        private ForestBiome forestBiome = new ForestBiome();
        private DesertBiome desertBiome = new DesertBiome();

        private double temperatureDetail = 0.0075D;
        private Noise2DPerlin temperatureFunction = new Noise2DPerlin();

        private double moistureDetail = 0.0075D;
        private Noise2DPerlin moistureFunction = new Noise2DPerlin(25555);

        private BiomeProvider biomeProvider;

        private Biome[] registeredBiomes;
        private const int activeBiomes = 3;
        private int seaLevel = 50;

        public WorldGenerator()
        {
            registeredBiomes = new Biome[activeBiomes]
            {
                mountainBiome,
                desertBiome,
                forestBiome,
            };

            biomeProvider = new BiomeProvider(registeredBiomes);
        }

        public Chunk GenerateBlocksForChunkAt(int chunkX, int chunkY)
        {
            Chunk generatedChunk = new Chunk(chunkX, chunkY);

            double temperatureXOffset = 0;
            double temperatureYOffset = 0;
            temperatureYOffset = chunkX * Constants.CHUNK_SIZE * temperatureDetail;

            double moistureXOffset = 0;
            double moistureYOffset = 0;
            moistureYOffset = chunkX * Constants.CHUNK_SIZE * moistureDetail;

            for (int i = 0; i < Constants.CHUNK_SIZE; i++)
            {
                temperatureXOffset = chunkY * Constants.CHUNK_SIZE * temperatureDetail;
                moistureXOffset = chunkY * Constants.CHUNK_SIZE * moistureDetail;

                for (int j = 0; j < Constants.CHUNK_SIZE; j++)
                {
                    double temperature = temperatureFunction.GetValuePositive(temperatureXOffset, temperatureYOffset);
                    double moisture = moistureFunction.GetValuePositive(moistureXOffset, moistureYOffset);

                    WeightedBiome[] biomes = biomeProvider.GetBiomeMemberships(temperature, moisture);
                    double biomeHeightAddon = 0;

                    WeightedBiome bestBiome = biomes[0];
                    foreach (WeightedBiome wBiome in biomes)
                    {
                        if (bestBiome.percentage < wBiome.percentage)
                        {
                            bestBiome = wBiome;
                        }
                        biomeHeightAddon += wBiome.percentage * wBiome.biome.OffsetAt(chunkX, chunkY, i, j);
                    }
                    int totalHeight = seaLevel + (int)biomeHeightAddon;

                    generatedChunk.AddBlock(i, totalHeight, j, bestBiome.biome.topBlock.GetNewDefaultState());
                    if (totalHeight > 0)
                    {
                        for (int k = totalHeight - 1; k > 0; k--)
                        {
                            generatedChunk.AddBlock(i, k, j, Blocks.Stone.GetNewDefaultState());
                        }
                    }

                    temperatureXOffset += temperatureDetail;
                    moistureXOffset += moistureDetail;
                }

                temperatureYOffset += temperatureDetail;
                moistureYOffset += moistureDetail;
            }

            return generatedChunk;
        }
    }
}
