namespace Minecraft
{
    class ForestBiome : Biome
    {
        private readonly Noise2DPerlin noisePerlin = new Noise2DPerlin();
        private const double terrainDetail = 0.005D;
        private const double heightVariation = 32;

        protected override void DefineProperties()
        {
            baseHeight = 0;
            temperature = 0.1D;
            moisture = 0.9D;
            topBlock = Blocks.Grass;
            gradiantBlock = Blocks.Dirt;
            decorator = new ForestDecorator();
        }

        public override double OffsetAt(int cx, int cy, int x, int y)
        {
            double dy = cx * Constants.CHUNK_SIZE * terrainDetail + x * terrainDetail;
            double dx = cy * Constants.CHUNK_SIZE * terrainDetail + y * terrainDetail;
            return baseHeight + noisePerlin.GetValuePositive(dx, dy) * heightVariation;
        }
    }
}
