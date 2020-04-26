using System.IO;

namespace Minecraft
{
    class BlockStateWheat : BlockState
    {
        public ushort maturity = 0;
        public float elapsedTimeSinceLastGrowth = 0;

        public override Block GetBlock()
        {
            return Blocks.Wheat;
        }

        public override void ToStream(NetBufferedStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteUInt16(maturity);
            bufferedStream.WriteFloat(elapsedTimeSinceLastGrowth);
        }

        public override void FromStream(BinaryReader reader)
        {
            base.FromStream(reader);
            maturity = reader.ReadUInt16();
            elapsedTimeSinceLastGrowth = reader.ReadSingle();
        }

        public override int PayloadSize() => 2 + 4;

        public override void ExtractFromByteStream(byte[] bytes, int source)
        {
            maturity = DataConverter.BytesToUInt16(bytes, source);
            elapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, source + 2);
        }
    }
}
