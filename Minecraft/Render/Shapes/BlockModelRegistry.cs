namespace Minecraft
{
    class BlockModelRegistry
    {
        public BlockModel[] Models { get; private set; }
        private readonly BlockModels blockModels;

        public BlockModelRegistry(TextureAtlas textureAtlas)
        {
            blockModels = new BlockModels(textureAtlas);
            RegisterBlockModels();
        }

        private void RegisterBlockModels()
        {
            Models = new BlockModel[Blocks.Count + 1];

            Models[Blocks.Dirt.ID] = blockModels.Dirt;
            Models[Blocks.Stone.ID] = blockModels.Stone;
            Models[Blocks.Flower.ID] = blockModels.Flower;
            Models[Blocks.Tnt.ID] = blockModels.Tnt;
            Models[Blocks.Grass.ID] = blockModels.Grass;
            Models[Blocks.Sand.ID] = blockModels.Sand;
            Models[Blocks.SugarCane.ID] = blockModels.SugarCane;
            Models[Blocks.Wheat.ID] = blockModels.Wheat;
            Models[Blocks.SandStone.ID] = blockModels.SandStone;
            Models[Blocks.GrassBlade.ID] = blockModels.GrassBlade;
            Models[Blocks.DeadBush.ID] = blockModels.DeadBush;
            Models[Blocks.Cactus.ID] = blockModels.Cactus;
            Models[Blocks.OakLog.ID] = blockModels.OakLog;
            Models[Blocks.OakLeaves.ID] = blockModels.OakLeaves;
            Models[Blocks.Gravel.ID] = blockModels.Gravel;
        }
    }
}
