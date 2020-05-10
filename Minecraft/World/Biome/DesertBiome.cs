namespace Minecraft
{
    class DesertBiome : Biome
    {
        private readonly Noise2DPerlinOctave noiseOctave = new Noise2DPerlinOctave(1);
        private const double terrainDetail = 0.0005D;
        private const double heightVariation = 16;

        protected override void DefineProperties()
        {
            BaseHeight = 0;
            Temeprature = 0.5D;
            Moisture = 0.1D;
            TopBlock = Blocks.Sand;
            GradiantBlock = Blocks.SandStone;
            Decorator = new BarrenDecorator();
        }

        public override double OffsetAt(int cx, int cy, int x, int y)
        {
            double chunkDim = 16; 
            double dy = cx * chunkDim * terrainDetail + x * terrainDetail;
            double dx = cy * chunkDim * terrainDetail + y * terrainDetail;
            return BaseHeight + noiseOctave.GetValuePositive(dx, dy) * heightVariation;
        }
    }
}

