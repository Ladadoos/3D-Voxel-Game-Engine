using System;

namespace Minecraft
{
    abstract class NoiseGenerator
    {
        public int seed { get; private set; }

        public NoiseGenerator()
        {
            Random random = new Random();
            seed = random.Next(10000);
        }

        public NoiseGenerator(int seed)
        {
            this.seed = seed;
        }

        public abstract double GetValuePure(double x, double y);

        public double GetValuePositive(double x, double y)
        {
            double value = (GetValuePure(x + seed, y + seed) + 1) / 2;
            return value < 0 ? 0.00001D : value;
        }
    }
}
