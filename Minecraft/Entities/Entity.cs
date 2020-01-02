using OpenTK;

namespace Minecraft
{
    abstract class Entity
    {
        public int id;
        public EntityType entityType { get; private set; }

        public Vector3 position;

        public Entity(int id, Vector3 position, EntityType entityType)
        {
            this.id = id;
            this.position = position;
            this.entityType = entityType;
        }
    }
}
