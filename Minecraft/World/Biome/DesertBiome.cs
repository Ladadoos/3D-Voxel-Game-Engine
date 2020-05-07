namespace Minecraft
{
    class DesertBiome : Biome
    {
        private readonly Noise2DPerlinOctave noiseOctave = new Noise2DPerlinOctave(1);
        private const double terrainDetail = 0.0005D;
        private const double heightVariation = 16;

        protected override void DefineProperties()
        {
            baseHeight = 0;
            temperature = 0.5D;
            moisture = 0.1D;
            topBlock = Blocks.Sand;
            gradiantBlock = Blocks.SandStone;
            decorator = new BarrenDecorator();
        }

        public override double OffsetAt(int cx, int cy, int x, int y)
        {
            double dy = cx * Constants.CHUNK_SIZE * terrainDetail + x * terrainDetail;
            double dx = cy * Constants.CHUNK_SIZE * terrainDetail + y * terrainDetail;
            return baseHeight + noiseOctave.GetValuePositive(dx, dy) * heightVariation;
        }
    }
}

