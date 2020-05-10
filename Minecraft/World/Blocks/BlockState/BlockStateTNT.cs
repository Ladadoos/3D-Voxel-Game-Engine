using System.IO;

namespace Minecraft
{
    class BlockStateTNT : BlockState
    {
        public float elapsedSecondsSinceTrigger;
        public bool triggeredByTnt;
        public bool triggered;
        public Vector3i blockPos;

        public override Block GetBlock()
        {
            return Blocks.Tnt;
        }

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(elapsedSecondsSinceTrigger);
            bufferedStream.WriteBool(triggeredByTnt);
            bufferedStream.WriteBool(triggered);
        }

        public override int PayloadSize() => sizeof(float) + sizeof(bool) + sizeof(bool);
 
        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            elapsedSecondsSinceTrigger = DataConverter.BytesToFloat(bytes, ref head);
            triggeredByTnt = DataConverter.BytesToBool(bytes, ref head);
            triggered = DataConverter.BytesToBool(bytes, ref head);
        }

        public override string ToString()
        {
            return base.ToString() + " dt_trigger=" + elapsedSecondsSinceTrigger + " triggered=" + triggered + " triggered_by_tnt=" + triggeredByTnt;
        }
    }
}
