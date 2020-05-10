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

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(elapsedTimeSinceLastGrowth);
        }

        public override int PayloadSize() => sizeof(float);

        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            elapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, ref head);
        }
    }
}
