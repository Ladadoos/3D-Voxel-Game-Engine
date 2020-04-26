using System.IO;

namespace Minecraft
{
    class BlockStateSugarCane : BlockState
    {
        public float elapsedTimeSinceLastGrowth;

        public override Block GetBlock()
        {
            return Blocks.SugarCane;
        }

        public override void ToStream(NetBufferedStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(elapsedTimeSinceLastGrowth);
        }

        public override void FromStream(BinaryReader reader)
        {
            base.FromStream(reader);
            elapsedTimeSinceLastGrowth = reader.ReadSingle();
        }

        public override int PayloadSize() => 4;

        public override void ExtractFromByteStream(byte[] bytes, int source)
        {
            elapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, source);
        }
    }
}
