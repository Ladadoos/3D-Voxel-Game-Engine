namespace Minecraft
{
    class MountainBiome : Biome
    {
        private readonly Noise2DPerlin noiseOctave = new Noise2DPerlin();
        private const double terrainDetail = 0.005D;
        private const double heightVariation = 64;

        protected override void DefineProperties()
        {
            BaseHeight = 8;
            Temeprature = 0.9D;
            Moisture = 0.5D;
            TopBlock = Blocks.Stone;
            GradiantBlock = Blocks.Stone;
            Decorator = new RockyDecorator();
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
