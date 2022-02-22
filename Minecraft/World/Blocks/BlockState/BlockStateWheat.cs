namespace Minecraft
{
    class BlockStateWheat : BlockState
    {
        public ushort Maturity { get; set; } = 0;
        public float ElapsedTimeSinceLastGrowth { get; set; } = 0;

        public override Block GetBlock()
        {
            return Blocks.Wheat;
        }

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteUInt16(Maturity);
            bufferedStream.WriteFloat(ElapsedTimeSinceLastGrowth);
        }

        public override int PayloadSize() => sizeof(ushort) + sizeof(float);

        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            Maturity = DataConverter.BytesToUInt16(bytes, ref head);
            ElapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, ref head);
        }
    }
}
