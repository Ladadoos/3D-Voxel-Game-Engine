using System.IO;

using ProtoBuf;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(1, typeof(BlockStateDirt))]
    [ProtoInclude(2, typeof(BlockStateAir))]
    [ProtoInclude(3, typeof(BlockStateStone))]
    [ProtoInclude(4, typeof(BlockStateFlower))]
    [ProtoInclude(5, typeof(BlockStateTNT))]
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

    [ProtoContract(SkipConstructor = true)]
    class BlockStateDirt : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Dirt;
        }
    }

    [ProtoContract(SkipConstructor = true)]
    class BlockStateAir : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Air;
        }
    }

    [ProtoContract(SkipConstructor = true)]
    class BlockStateStone : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Stone;
        }
    }

    [ProtoContract(SkipConstructor = true)]
    class BlockStateFlower : BlockState
    {
        public override Block GetBlock()
        {
            return Blocks.Flower;
        }
    }

    [ProtoContract(SkipConstructor = true)]
    class BlockStateTNT : BlockState
    {
        [ProtoMember(1)]
        public float elapsedSecondsSinceTrigger;
        [ProtoMember(2)]
        public bool triggeredByTnt;
        [ProtoMember(3)]
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
