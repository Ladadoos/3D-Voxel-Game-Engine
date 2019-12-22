using OpenTK;

namespace Minecraft
{
    abstract class BlockState
    {
        //Position is automatically set by the world once the block is placed.
        public Vector3 position;

        public Vector3 ChunkLocalPosition()
        {
            return new Vector3((int)position.X & 15, position.Y, (int)position.Z & 15);
        }

        public virtual void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(GetBlock().id);
            bufferedStream.WriteFloat(position.X);
            bufferedStream.WriteFloat(position.Y);
            bufferedStream.WriteFloat(position.Z);
        }

        public abstract Block GetBlock();
    }

    class BlockStateDirt : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Dirt;
        }
    }

    class BlockStateAir : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Air;
        }
    }

    class BlockStateStone : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Stone;
        }
    }

    class BlockStateFlower : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Flower;
        }
    }

    class BlockStateTNT : BlockState
    {
        public float elapsedSecondsSinceTrigger;
        public bool triggeredByTnt;
        public bool triggered;

        public override Block GetBlock()
        {
            return Blocks.Tnt;
        }
    }
}
