using System.Collections.Generic;

namespace Minecraft
{
    class EntityModelRegistry
    {
        public ReadOnlyDictionary<EntityType, EntityModel> Models { get; private set; }
        private readonly EntityModels entityModels;

        public EntityModelRegistry(TextureAtlas textureAtlas)
        {
            entityModels = new EntityModels(textureAtlas);
            RegisterEntityModels();
        }

        private void RegisterEntityModels()
        {
            Dictionary<EntityType, EntityModel> registry = new Dictionary<EntityType, EntityModel>
            {
                { EntityType.Dummy, entityModels.Dummy },
                { EntityType.Player, entityModels.Dummy },
            };
            Models = new ReadOnlyDictionary<EntityType, EntityModel>(registry);
        }
    }
}
