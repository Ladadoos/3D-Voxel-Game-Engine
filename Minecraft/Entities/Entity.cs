using OpenTK;

namespace Minecraft
{
    abstract class Entity
    {
        public int id { get; private set; }
        public EntityType entityType { get; private set; }

        public Vector3 position;

        public Entity(int id, EntityType entityType)
        {
            this.id = id;
            this.entityType = entityType;
        }
    }
}
