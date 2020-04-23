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

        public override void ToStream(NetBufferedStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(elapsedSecondsSinceTrigger);
            bufferedStream.WriteBool(triggeredByTnt);
            bufferedStream.WriteBool(triggered);
        }

        public override void FromStream(BinaryReader reader)
        {
            base.FromStream(reader);
            elapsedSecondsSinceTrigger = reader.ReadSingle();
            triggeredByTnt = reader.ReadBoolean();
            triggered = reader.ReadBoolean();
        }

        public override int PayloadSize() => 4 + 1 + 1;
 
        public override void ExtractFromByteStream(byte[] bytes, int source)
        {
            elapsedSecondsSinceTrigger = DataConverter.BytesToFloat(bytes, source);
            triggeredByTnt = DataConverter.BytesToBool(bytes, source + 4);
            triggered = DataConverter.BytesToBool(bytes, source + 5);
        }

        public override string ToString()
        {
            return base.ToString() + " dt_trigger=" + elapsedSecondsSinceTrigger + " triggered=" + triggered + " triggered_by_tnt=" + triggeredByTnt;
        }
    }
}
