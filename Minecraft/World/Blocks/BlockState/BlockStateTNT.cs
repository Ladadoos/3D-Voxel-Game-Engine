using System;

namespace Minecraft
{
    class BlockStateTNT : BlockState, ILightSource
    {
        private static Random r = new Random();
        public Vector3i LightColor { get; private set; }

        public float ElapsedSecondsSinceTrigger { get; set; }
        public ExplosionTrigger Trigger { get; set; }
        public Vector3i BlockPosition { get; set; }

        public BlockStateTNT()
        {
            LightColor = new Vector3i(r.Next(15), r.Next(15), r.Next(15));
        }

        public override Block GetBlock()
        {
            return Blocks.Tnt;
        }

        public override void ToStream(BufferedDataStream bufferedStream)
        {
            base.ToStream(bufferedStream);
            bufferedStream.WriteFloat(ElapsedSecondsSinceTrigger);
            bufferedStream.WriteByte((byte)Trigger);
            bufferedStream.WriteVector3i(LightColor);
        }

        public override int PayloadSize() => sizeof(float) + sizeof(byte) + sizeof(float) * 3;
 
        public override void ExtractFromByteStream(byte[] bytes, ref int head)
        {
            ElapsedSecondsSinceTrigger = DataConverter.BytesToFloat(bytes, ref head);
            Trigger = DataConverter.BytesToByteStruct<ExplosionTrigger>(bytes, ref head);
            LightColor = DataConverter.BytesToVector3i(bytes, ref head);
        }

        public override string ToString()
        {
            return base.ToString() + " dt_trigger=" + ElapsedSecondsSinceTrigger + " triggered=" + Trigger;
        }
    }
}
