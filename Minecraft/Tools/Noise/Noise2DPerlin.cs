using LibNoise;

namespace Minecraft
{
    class Noise2DPerlin : NoiseGenerator
    {
        private readonly Perlin perlin = new Perlin();

        public Noise2DPerlin(int seed) : base(seed)
        {
        }

        public Noise2DPerlin()
        {
        }
        
        public override double GetValuePure(double x, double y)
        {
            return perlin.GetValue(x + seed, 1, y + seed);
        }
    }
}
