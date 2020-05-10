using System;

namespace Minecraft
{
    abstract class NoiseGenerator
    {
        public int Seed { get; private set; }

        protected NoiseGenerator()
        {
            Random random = new Random();
            Seed = random.Next(10000);
        }

        protected NoiseGenerator(int seed)
        {
            this.Seed = seed;
        }

        public abstract double GetValuePure(double x, double y);

        public double GetValuePositive(double x, double y)
        {
            double value = (GetValuePure(x + Seed, y + Seed) + 1) / 2;
            return value < 0 ? 0.00001D : value;
        }
    }
}
