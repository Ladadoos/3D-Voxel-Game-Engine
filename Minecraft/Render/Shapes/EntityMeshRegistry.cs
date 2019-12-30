using System.Collections.Generic;

namespace Minecraft
{
    class EntityMeshRegistry
    {
        public ReadOnlyDictionary<EntityType, Model> models;
        private EntityModelRegistry entityModels;

        public EntityMeshRegistry(TextureAtlas textureAtlas)
        {
            entityModels = new EntityModelRegistry(textureAtlas);
            RegisterEntityModels();
        }

        private void RegisterEntityModels()
        {
            EntityMeshGenerator entityMeshGenerator = new EntityMeshGenerator(entityModels);

            Dictionary<EntityType, Model> registry = new Dictionary<EntityType, Model>();
            foreach (KeyValuePair<EntityType, EntityModel> entityResource in entityModels.models)
            {
                registry.Add(entityResource.Key, entityMeshGenerator.GenerateMeshFor(entityResource.Value));
            }
            models = new ReadOnlyDictionary<EntityType, Model>(registry);
        }
    }
}
