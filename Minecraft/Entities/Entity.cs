using OpenTK;

namespace Minecraft
{
    abstract class Entity
    {
        public int id;
        public EntityType entityType { get; private set; }

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public AxisAlignedBox hitbox { get; protected set; }

        protected float width, height, length;

        public delegate void OnDespawned();
        public event OnDespawned OnDespawnedHandler;

        public delegate void OnChunkChanged(World world, Vector2 gridPos);
        public event OnChunkChanged OnChunkChangedHandler;

        private Vector2 previousChunkPos = new Vector2(float.MaxValue, float.MaxValue);

        protected Entity(int id, Vector3 position, EntityType entityType)
        {
            this.id = id;
            this.position = position;
            this.velocity = Vector3.Zero;
            this.acceleration = Vector3.Zero;
            this.entityType = entityType;
            SetInitialDimensions();
            InitialAxisAlignedBox();
        }

        public void RaiseOnDespawned() => OnDespawnedHandler?.Invoke();

        protected abstract void SetInitialDimensions();

        private void InitialAxisAlignedBox()
        {
            Vector3 max = new Vector3(position.X + width, position.Y + height, position.Z + length);
            hitbox = new AxisAlignedBox(position, max);
        }

        protected void UpdateAxisAlignedBox()
        {
            Vector3 max = new Vector3(position.X + width, position.Y + height, position.Z + length);
            hitbox.SetDimensions(position, max);
        }

        /// <summary> Called as often as possible </summary>
        public virtual void Update(float deltaTime, World world)
        {
            UpdateAxisAlignedBox();
        }

        /// <summary> Called every tick </summary>
        public virtual void Tick(float deltaTime, World world)
        {
            Vector2 chunkPosition = World.GetChunkPosition(position.X, position.Z);
            if (previousChunkPos != chunkPosition)
            {
                OnChunkChangedHandler?.Invoke(world, chunkPosition);
            }
            previousChunkPos = chunkPosition;
        }
    }
}
