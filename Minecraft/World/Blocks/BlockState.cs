using System;
using System.IO;

namespace Minecraft
{
    [Serializable]
    abstract class BlockState
    {
        public abstract Block GetBlock();

        public BlockState ShallowCopy()
        {
            return (BlockState)MemberwiseClone();
        }

        public virtual void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(GetBlock().id);
        }

        public virtual void FromStream(BinaryReader reader) { }
    }

    [Serializable]
    class BlockStateDirt : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Dirt;
        }
    }

    [Serializable]
    class BlockStateAir : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Air;
        }
    }

    [Serializable]
    class BlockStateStone : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Stone;
        }
    }

    [Serializable]
    class BlockStateFlower : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Flower;
        }
    }

    [Serializable]
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
