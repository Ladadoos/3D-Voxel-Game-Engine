using OpenTK;

namespace Minecraft
{
    abstract class Entity
    {
        public int ID { get; set; }
        public EntityType EntityType { get; private set; }

        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public AxisAlignedBox Hitbox { get; protected set; }

        protected float width, height, length;

        public delegate void OnDespawned();
        public event OnDespawned OnDespawnedHandler;

        public delegate void OnChunkChanged(World world, Vector2 gridPos);
        public event OnChunkChanged OnChunkChangedHandler;

        private Vector2 previousChunkPos = new Vector2(float.MaxValue, float.MaxValue);

        protected Entity(int id, Vector3 position, EntityType entityType)
        {
            ID = id;
            Position = position;
            Velocity = Vector3.Zero;
            Acceleration = Vector3.Zero;
            EntityType = entityType;
            SetInitialDimensions();
            InitialAxisAlignedBox();
        }

        public void RaiseOnDespawned() => OnDespawnedHandler?.Invoke();

        protected abstract void SetInitialDimensions();

        private void InitialAxisAlignedBox()
        {
            Vector3 max = new Vector3(Position.X + width, Position.Y + height, Position.Z + length);
            Hitbox = new AxisAlignedBox(Position, max);
        }

        protected void UpdateAxisAlignedBox()
        {
            Vector3 max = new Vector3(Position.X + width, Position.Y + height, Position.Z + length);
            Hitbox.SetDimensions(Position, max);
        }

        /// <summary> Called as often as possible </summary>
        public virtual void Update(float deltaTime, World world)
        {
            UpdateAxisAlignedBox();
        }

        /// <summary> Called every tick </summary>
        public virtual void Tick(float deltaTime, World world)
        {
            Vector2 chunkPosition = World.GetChunkPosition(Position.X, Position.Z);
            if (previousChunkPos != chunkPosition)
            {
                OnChunkChangedHandler?.Invoke(world, chunkPosition);
            }
            previousChunkPos = chunkPosition;
        }
    }
}
