using System.IO;

namespace Minecraft
{
    abstract class BlockState
    {
        public abstract Block GetBlock();

        public BlockState ShallowCopy()
        {
            return (BlockState)MemberwiseClone();
        }

        public virtual void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUInt16(GetBlock().id);
        }

        public virtual void FromStream(BinaryReader reader) { }

        public virtual int ByteSize()
        {
            return 2;
        }

        public virtual void ExtractFromByteStream(byte[] bytes, int source) { }

        public override string ToString()
        {
            return GetType().ToString();
        }
    }
}
