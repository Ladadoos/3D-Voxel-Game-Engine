namespace Minecraft
{
    class EntityModels
    {
        public readonly EntityModel Dummy;

        public EntityModels(TextureAtlas textureAtlas)
        {
            Dummy = new DummyEntityModel(textureAtlas);
        }
    }
}
