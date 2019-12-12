using OpenTK;

namespace Minecraft
{
    abstract class Block
    {
        public static readonly Block Air = new BlockAir();
        public static readonly Block Dirt = new BlockDirt();

        public BlockType blockType { get; protected set; }

        public abstract BlockState GetNewDefaultState();

        public virtual bool OnInteract(BlockState blockstate, Game game)
        {
            return false;
        }

        public virtual void OnAdded(BlockState blockstate, Game game) { }

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
        public BlockAir()
        {
            blockType = BlockType.Air;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateAir();
        }

        public override AABB[] GetCollisionBox(BlockState state)
        {
            return new AABB[0];
        }
    }

    class BlockDirt : Block
    {
        public BlockDirt()
        {
            blockType = BlockType.Dirt;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDirt();
        }

        public override bool OnInteract(BlockState blockstate, Game game)
        {
            BlockStateDirt state = (BlockStateDirt)blockstate;
            System.Console.WriteLine("Interacted with dirt at " + state.position);
            return true;
        }
    }
}
