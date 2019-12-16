namespace Minecraft
{
    class BlockModels
    {
        public readonly BlockModel Dirt;
        public readonly BlockModel Stone;
        public readonly BlockModel Flower;
        public readonly BlockModel Tnt;

        public BlockModels(TextureAtlas textureAtlas)
        {
            Dirt = new BlockModelDirt(textureAtlas);
            Stone = new BlockModelStone(textureAtlas);
            Flower = new BlockModelFlower(textureAtlas);
            Tnt = new BlockModelTNT(textureAtlas);
        }
    }
}
