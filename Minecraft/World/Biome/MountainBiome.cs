namespace Minecraft
{
    class MountainBiome : Biome
    {
        private Noise2DPerlin noiseOctave = new Noise2DPerlin();
        private double terrainDetail = 0.005D;
        private double heightVariation = 64;

        protected override void DefineProperties()
        {
            baseHeight = 8;
            temperature = 0.9D;
            moisture = 0.5D;
            topBlock = Blocks.Stone;
            gradiantBlock = Blocks.Stone;
        }

        public override double OffsetAt(int cx, int cy, int x, int y)
        {
            double dy = cx * Constants.CHUNK_SIZE * terrainDetail + x * terrainDetail;
            double dx = cy * Constants.CHUNK_SIZE * terrainDetail + y * terrainDetail;
            return baseHeight + noiseOctave.GetValuePositive(dx, dy) * heightVariation;
        }
    }
}
