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

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteUInt16(maturity);
            bufferedStream.WriteFloat(elapsedTimeSinceLastGrowth);
        }

        public override int PayloadSize() => sizeof(ushort) + sizeof(float);

        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            maturity = DataConverter.BytesToUInt16(bytes, ref head);
            elapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, ref head);
        }
    }
}
