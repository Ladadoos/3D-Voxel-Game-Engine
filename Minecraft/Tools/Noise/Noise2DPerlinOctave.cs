using LibNoise;

namespace Minecraft
{
    class Noise2DPerlinOctave : NoiseGenerator
    {
        private int octaves;
        private Perlin[] perlins;

        public Noise2DPerlinOctave(int octaves, int seed) : base(seed)
        {
            this.octaves = octaves;
            InstantiatePerlinFunctions();
        }

        public Noise2DPerlinOctave(int octaves)
        {
            this.octaves = octaves;
            InstantiatePerlinFunctions();
        }

        private void InstantiatePerlinFunctions()
        {
            perlins = new Perlin[octaves];
            for(int i = 0; i < octaves; i++)
            {
                perlins[i] = new Perlin();
            }
        }

        public override double GetValuePure(double x, double y)
        {
            double sum = 0;
            double oct = 1;
            for(int i = 0; i < octaves; i++)
            {
                sum += oct * perlins[i].GetValue(i * x + seed, 1, i * y + seed);
                oct /= 2;
            }
            return sum;
        }
    }
}
