using OpenTK;

namespace Minecraft
{
    abstract class BlockState
    {
        public Block block { get; protected set; }

        //Position is automatically set by the world once the block is placed.
        public Vector3 position;

        public BlockState Clone()
        {
            return (BlockState)MemberwiseClone();
        }

        public Vector3 ChunkLocalPosition()
        {
            return new Vector3((int)position.X & 15, position.Y, (int)position.Z & 15);
        }
    }

    class BlockStateDirt : BlockState
    {
        public BlockStateDirt() : base()
        {
            block = Block.Dirt;
        }
    }

    class BlockStateAir : BlockState
    {
        public BlockStateAir() : base()
        {
            block = Block.Air;
        }
    }

    class BlockStateStone : BlockState
    {
        public BlockStateStone() : base()
        {
            block = Block.Stone;
        }
    }

    class BlockStateFlower : BlockState
    {
        public BlockStateFlower() : base()
        {
            block = Block.Flower;
        }
    }
}
