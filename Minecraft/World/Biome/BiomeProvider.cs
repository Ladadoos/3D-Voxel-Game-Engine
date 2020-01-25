using System;

namespace Minecraft
{
    struct WeightedBiome
    {
        public double percentage;
        public Biome biome;
    };

    class BiomeProvider
    {
        private double[] temperatureCache;
        private Biome[] registeredBiomes;

        public BiomeProvider(Biome[] registeredBiomes)
        {
            this.registeredBiomes = registeredBiomes;
            temperatureCache = new double[registeredBiomes.Length];
        }

        public WeightedBiome[] GetBiomeMemberships(double temperature, double moisture)
        {
            double sum = 0;
            for (int i = 0; i < registeredBiomes.Length; i++)
            {
                double dtTemp = (Math.Abs(registeredBiomes[i].temperature - temperature) / temperature);
                double dtMoist = (Math.Abs(registeredBiomes[i].moisture - moisture) / moisture);
                double dt = 1 / (Maths.Pow((dtTemp + dtMoist), 3));
                temperatureCache[i] = dt;
                sum += dt;
            }

            WeightedBiome[] weightedBiomes = new WeightedBiome[registeredBiomes.Length];
            for (int i = 0; i < registeredBiomes.Length; i++)
            {
                weightedBiomes[i] = new WeightedBiome()
                {
                    percentage = temperatureCache[i] / sum,
                    biome = registeredBiomes[i]
                };
            }
            return weightedBiomes;
        }
    }
}
