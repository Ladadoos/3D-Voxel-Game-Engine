using System.IO;

namespace Minecraft
{
    abstract class BlockState
    {
        public abstract Block GetBlock();

        public virtual void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(GetBlock().id);
        }

        public virtual void FromStream(BinaryReader reader) { }
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

        public override void ToStream(NetBufferedStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(elapsedSecondsSinceTrigger);
            bufferedStream.WriteBool(triggeredByTnt);
            bufferedStream.WriteBool(triggered);
        }

        public override void FromStream(BinaryReader reader)
        {
            base.FromStream(reader);
            elapsedSecondsSinceTrigger = reader.ReadSingle();
            triggeredByTnt = reader.ReadBoolean();
            triggered = reader.ReadBoolean();
        }
    }
}
