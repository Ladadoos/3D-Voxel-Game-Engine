using LibNoise;

namespace Minecraft 
{
    class Noise3DPerlin
    {
        private readonly Perlin perlin = new Perlin();
        private readonly int seed;

        public Noise3DPerlin(int seed)
        {
            this.seed = seed;
        }

        public double GetValue(float x, float y, float z)
        {
            return perlin.GetValue(seed + x, seed + y, seed + z);
        }
    }
}
