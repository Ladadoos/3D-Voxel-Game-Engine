using OpenTK;

namespace Minecraft
{
    abstract class Block
    {
        public int id { get; private set; }
        protected readonly AABB[] emptyAABB = new AABB[0];
        public bool isTickable { get; protected set; }

        public Block(int id)
        {
            this.id = id;
        }

        public abstract BlockState GetNewDefaultState();

        public virtual bool OnInteract(BlockState blockstate, World world)
        {
            return false;
        }

        public virtual bool CanAddBlockAt(World world, Vector3 intPosition)
        {
            return true;
        }

        public virtual void OnTick(BlockState blockState, World world, float deltaTime) { }

        public virtual void OnAdded(BlockState blockstate, World world) { }

        public virtual AABB[] GetCollisionBox(BlockState state)
        {
            return new AABB[] { GetFullBlockCollision(state) };
        }

        public static AABB GetFullBlockCollision(BlockState state)
        {
            return new AABB(new Vector3(state.position.X, state.position.Y, state.position.Z),
                new Vector3(state.position.X + Constants.CUBE_SIZE, state.position.Y + Constants.CUBE_SIZE, state.position.Z + Constants.CUBE_SIZE));
        }
    }

    class BlockAir : Block
    {
        public BlockAir(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateAir();
        }

        public override AABB[] GetCollisionBox(BlockState state)
        {
            return emptyAABB;
        }
    }

}
