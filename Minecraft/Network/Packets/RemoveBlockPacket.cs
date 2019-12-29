using OpenTK;

namespace Minecraft
{
    class RemoveBlockPacket : Packet
    {
        public Vector3 position { get; private set; }

        public RemoveBlockPacket(Vector3 position) : base(PacketType.RemoveBlock)
        {
            this.position = position;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessRemoveBlockPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteFloat(position.X);
            bufferedStream.WriteFloat(position.Y);
            bufferedStream.WriteFloat(position.Z);
        }
    }
}
