namespace Minecraft
{
    class BlockModels
    {
        public readonly BlockModel Dirt;
        public readonly BlockModel Stone;
        public readonly BlockModel Flower;
        public readonly BlockModel Tnt;
        public readonly BlockModel Grass;
        public readonly BlockModel Sand;
        public readonly BlockModel SugarCane;

        public BlockModels(TextureAtlas textureAtlas)
        {
            Dirt = new BlockModelDirt(textureAtlas);
            Stone = new BlockModelStone(textureAtlas);
            Flower = new BlockModelFlower(textureAtlas);
            Tnt = new BlockModelTNT(textureAtlas);
            Grass = new BlockModelGrass(textureAtlas);
            Sand = new BlockModelSand(textureAtlas);
            SugarCane = new BlockModelSugarCane(textureAtlas);
        }
    }
}
