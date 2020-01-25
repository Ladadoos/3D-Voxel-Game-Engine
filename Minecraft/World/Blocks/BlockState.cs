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
    [ProtoInclude(6, typeof(BlockStateGrass))]
    [ProtoInclude(7, typeof(BlockStateSand))]
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
}
