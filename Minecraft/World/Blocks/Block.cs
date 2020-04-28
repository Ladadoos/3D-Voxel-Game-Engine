using OpenTK;

namespace Minecraft
{
    abstract class Block
    {
        public ushort id { get; private set; }
        public bool isTickable { get; protected set; }
        public bool isInteractable { get; protected set; }
        protected readonly AxisAlignedBox[] emptyAABB = new AxisAlignedBox[0];

        public Block(ushort id)
        {
            this.id = id;
        }

        public abstract BlockState GetNewDefaultState();

        public virtual void OnInteract(BlockState blockstate, Vector3i blockPos, World world) { }

        public virtual bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            return true;
        }

        public virtual void OnTick(BlockState blockState, World world, Vector3i blockPos, float deltaTime) { }

        public virtual void OnAdd(BlockState blockState, World world, Vector3i blockPos)
        {
            NotifyNeighbours(blockState, world, blockPos);
        }

        public virtual void OnDestroy(BlockState blockState, World world, Vector3i blockPos)
        {
            NotifyNeighbours(blockState, world, blockPos);
        }

        public virtual void OnNotify(BlockState blockState, BlockState sourceBlockState, World world, Vector3i blockPos, Vector3i sourceBlockPos) { }

        public virtual AxisAlignedBox[] GetSelectionBox(BlockState state, Vector3i blockPos)
        {
            return new AxisAlignedBox[] { GetFullBlockCollision(blockPos) };
        }

        public virtual AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return new AxisAlignedBox[] { GetFullBlockCollision(blockPos) };
        }

        public static AxisAlignedBox GetFullBlockCollision(Vector3i blockPos)
        {
            return new AxisAlignedBox(new Vector3(blockPos.X, blockPos.Y, blockPos.Z),
                new Vector3(blockPos.X + Constants.CUBE_SIZE, blockPos.Y + Constants.CUBE_SIZE, blockPos.Z + Constants.CUBE_SIZE));
        }

        protected void NotifyNeighbours(BlockState blockState, World world, Vector3i blockPos)
        {
            foreach(Vector3i neighbourPos in blockPos.GetSurroundingPositions())
            {
                BlockState state = world.GetBlockAt(neighbourPos);
                state?.GetBlock().OnNotify(state, blockState, world, neighbourPos, blockPos);
            }
        }
    }
}
