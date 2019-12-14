namespace Minecraft
{
    class BlockModels
    {
        public readonly BlockModel Dirt;
        public readonly BlockModel Stone;
        public readonly BlockModel Flower;

        public BlockModels(TextureAtlas textureAtlas)
        {
            Dirt = new BlockModelDirt(textureAtlas);
            Stone = new BlockModelStone(textureAtlas);
            Flower = new BlockModelFlower(textureAtlas);
        }
    }
}
