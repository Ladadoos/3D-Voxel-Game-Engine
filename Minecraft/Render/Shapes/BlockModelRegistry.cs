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
                { Block.Dirt, blockModels.Dirt },
                { Block.Stone, blockModels.Stone },
                { Block.Flower, blockModels.Flower }
            };
            models = new ReadOnlyDictionary<Block, BlockModel>(registry);
        }
    }
}
