using OpenTK;

namespace Minecraft
{
    abstract class BlockState
    {
        public Block block { get; protected set; }

        //Position is automatically set by the world once the block is placed.
        public Vector3 position;

        public Vector3 ChunkLocalPosition()
        {
            return new Vector3((int)position.X & 15, position.Y, (int)position.Z & 15);
        }
    }

    class BlockStateDirt : BlockState
    {
        public BlockStateDirt() : base()
        {
            block = Blocks.Dirt;
        }
    }

    class BlockStateAir : BlockState
    {
        public BlockStateAir() : base()
        {
            block = Blocks.Air;
        }
    }

    class BlockStateStone : BlockState
    {
        public BlockStateStone() : base()
        {
            block = Blocks.Stone;
        }
    }

    class BlockStateFlower : BlockState
    {
        public BlockStateFlower() : base()
        {
            block = Blocks.Flower;
        }
    }

    class BlockStateTNT : BlockState
    {
        public float elapsedSecondsSinceTrigger;
        public bool triggeredByTnt;
        public bool triggered;

        public BlockStateTNT() : base()
        {
            block = Blocks.Tnt;
        }
    }
}
