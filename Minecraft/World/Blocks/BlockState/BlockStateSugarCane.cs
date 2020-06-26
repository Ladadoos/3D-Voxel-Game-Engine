using System.IO;

namespace Minecraft
{
    class BlockStateSugarCane : BlockState
    {
        public float ElapsedTimeSinceLastGrowth;

        public override Block GetBlock()
        {
            return Blocks.SugarCane;
        }

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(ElapsedTimeSinceLastGrowth);
        }

        public override int PayloadSize() => sizeof(float);

        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            ElapsedTimeSinceLastGrowth = DataConverter.BytesToFloat(bytes, ref head);
        }
    }
}
