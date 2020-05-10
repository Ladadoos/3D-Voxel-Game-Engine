namespace Minecraft
{
    class ForestBiome : Biome
    {
        private readonly Noise2DPerlin noisePerlin = new Noise2DPerlin();
        private const double terrainDetail = 0.005D;
        private const double heightVariation = 32;

        protected override void DefineProperties()
        {
            BaseHeight = 0;
            Temeprature = 0.1D;
            Moisture = 0.9D;
            TopBlock = Blocks.Grass;
            GradiantBlock = Blocks.Dirt;
            Decorator = new ForestDecorator();
        }

        public override double OffsetAt(int cx, int cy, int x, int y)
        {
            double chunkDim = 16;
            double dy = cx * chunkDim * terrainDetail + x * terrainDetail;
            double dx = cy * chunkDim * terrainDetail + y * terrainDetail;
            return BaseHeight + noisePerlin.GetValuePositive(dx, dy) * heightVariation;
        }
    }
}
