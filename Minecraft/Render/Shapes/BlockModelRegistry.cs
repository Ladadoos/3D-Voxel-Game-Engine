using System.Collections.Generic;

namespace Minecraft
{
    class BlockModelRegistry
    {
        public ReadOnlyDictionary<Block, BlockModel> models;
        private BlockModels blockModels;

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
                { Blocks.Wheat, blockModels.Wheat }
            };
            models = new ReadOnlyDictionary<Block, BlockModel>(registry);
        }
    }
}
