using System;

namespace Minecraft
{
    class BiomeProvider
    {
        private readonly double[] temperatureCache;
        private readonly BiomeMembership[] biomeMembershipCache;
        private readonly Biome[] registeredBiomes;

        public BiomeProvider(Biome[] registeredBiomes)
        {
            this.registeredBiomes = registeredBiomes;
            temperatureCache = new double[registeredBiomes.Length];
            biomeMembershipCache = new BiomeMembership[registeredBiomes.Length];
        }

        public BiomeMembership[] GetBiomeMemberships(double temperature, double moisture)
        {
            double sum = 0;
            for (int i = 0; i < registeredBiomes.Length; i++)
            {
                double dtTemp = (Math.Abs(registeredBiomes[i].temperature - temperature) / temperature);
                double dtMoist = (Math.Abs(registeredBiomes[i].moisture - moisture) / moisture);
                double sumDt = dtTemp + dtMoist;
                double dt = 1 / (sumDt * sumDt * sumDt);
                temperatureCache[i] = dt;
                sum += dt;
            }

            for (int i = 0; i < registeredBiomes.Length; i++)
            {
                biomeMembershipCache[i] = new BiomeMembership()
                {
                    percentage = temperatureCache[i] / sum,
                    biome = registeredBiomes[i]
                };
            }
            return biomeMembershipCache;
        }
    }
}
