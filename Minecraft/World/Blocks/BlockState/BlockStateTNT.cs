using ProtoBuf;
using System.IO;

namespace Minecraft
{
    [ProtoContract(SkipConstructor = true)]
    class BlockStateTNT : BlockState
    {
        [ProtoMember(1)]
        public float elapsedSecondsSinceTrigger;
        [ProtoMember(2)]
        public bool triggeredByTnt;
        [ProtoMember(3)]
        public bool triggered;

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
    }
}
