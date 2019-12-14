using System.Collections.Generic;

namespace Minecraft
{
    class BlockModelRegistry
    {
        public ReadOnlyDictionary<Block, BlockModel> modelRegistry;
        private BlockModels blockModels = new BlockModels();

        public BlockModelRegistry()
        {
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
            modelRegistry = new ReadOnlyDictionary<Block, BlockModel>(registry);
        }
    }
}
