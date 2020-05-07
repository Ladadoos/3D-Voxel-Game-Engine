using System.Collections.Generic;

namespace Minecraft
{
    class EntityMeshRegistry
    {
        public ReadOnlyDictionary<EntityType, VAOModel> models;
        private readonly EntityModelRegistry entityModels;

        public EntityMeshRegistry(TextureAtlas textureAtlas)
        {
            entityModels = new EntityModelRegistry(textureAtlas);
            RegisterEntityModels();
        }

        private void RegisterEntityModels()
        {
            EntityMeshGenerator entityMeshGenerator = new EntityMeshGenerator(entityModels);

            Dictionary<EntityType, VAOModel> registry = new Dictionary<EntityType, VAOModel>();
            foreach (KeyValuePair<EntityType, EntityModel> entityResource in entityModels.models)
            {
                registry.Add(entityResource.Key, entityMeshGenerator.GenerateMeshFor(entityResource.Value));
            }
            models = new ReadOnlyDictionary<EntityType, VAOModel>(registry);
        }
    }
}
