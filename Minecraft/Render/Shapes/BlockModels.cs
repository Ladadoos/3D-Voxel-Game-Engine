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
        public readonly BlockModel Wheat;
        public readonly BlockModel Sandstone;
        public readonly BlockModel GrassBlade;
        public readonly BlockModel DeadBush;
        public readonly BlockModel Cactus;
        public readonly BlockModel OakLog;
        public readonly BlockModel OakLeaves;
        public readonly BlockModel Gravel;

        public BlockModels(TextureAtlas textureAtlas)
        {
            Dirt = new BlockModelDirt(textureAtlas);
            Stone = new BlockModelStone(textureAtlas);
            Flower = new BlockModelFlower(textureAtlas);
            Tnt = new BlockModelTNT(textureAtlas);
            Grass = new BlockModelGrass(textureAtlas);
            Sand = new BlockModelSand(textureAtlas);
            SugarCane = new BlockModelSugarCane(textureAtlas);
            Wheat = new BlockModelWheat(textureAtlas);
            Sandstone = new BlockModelSandstone(textureAtlas);
            GrassBlade = new BlockModelGrassBlade(textureAtlas);
            DeadBush = new BlockModelDeadBush(textureAtlas);
            Cactus = new BlockModelCactus(textureAtlas);
            OakLog = new BlockModelOakLog(textureAtlas);
            OakLeaves = new BlockModelOakLeaves(textureAtlas);
            Gravel = new BlockModelGravel(textureAtlas);
        }
    }
}
