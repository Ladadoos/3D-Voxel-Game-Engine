using System.Collections.Generic;

namespace Minecraft
{
    class BlockModelRegistry
    {
        public ReadOnlyDictionary<Block, BlockModel> Models { get; private set; }
        private readonly BlockModels blockModels;

        public BlockModelRegistry(TextureAtlas textureAtlas)
        {
            blockModels = new BlockModels(textureAtlas);
            RegisterBlockModels();
        }

        private void RegisterBlockModels()
        {
            Dictionary<Block, BlockModel> registry = new Dictionary<Block, BlockModel>
            {
                { Blocks.Dirt, blockModels.Dirt },
                { Blocks.Stone, blockModels.Stone },
                { Blocks.Flower, blockModels.Flower },
                { Blocks.Tnt, blockModels.Tnt },
                { Blocks.Grass, blockModels.Grass },
                { Blocks.Sand, blockModels.Sand },
                { Blocks.SugarCane, blockModels.SugarCane },
                { Blocks.Wheat, blockModels.Wheat },
                { Blocks.SandStone, blockModels.Sandstone },
                { Blocks.GrassBlade, blockModels.GrassBlade },
                { Blocks.DeadBush, blockModels.DeadBush },
                { Blocks.Cactus, blockModels.Cactus },
                { Blocks.OakLog, blockModels.OakLog },
                { Blocks.OakLeaves, blockModels.OakLeaves },
                { Blocks.Gravel, blockModels.Gravel }
            };
            Models = new ReadOnlyDictionary<Block, BlockModel>(registry);
        }
    }
}
