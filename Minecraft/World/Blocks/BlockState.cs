using System.IO;

namespace Minecraft
{
    abstract class BlockState
    {
        public abstract Block GetBlock();

        public virtual void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUInt16(GetBlock().id);
        }

        public virtual void FromStream(BinaryReader reader) { }

        public virtual int PayloadSize() => 0;

        public virtual void ExtractFromByteStream(byte[] bytes, int source) { }

        public override string ToString() => GetType().ToString();
    }
}
