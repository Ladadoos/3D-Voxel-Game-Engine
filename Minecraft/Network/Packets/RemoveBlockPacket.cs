namespace Minecraft
{
    class RemoveBlockPacket : Packet
    {
        public Vector3i blockPos { get; private set; }

        public RemoveBlockPacket(Vector3i blockPos) : base(PacketType.RemoveBlock)
        {
            this.blockPos = blockPos;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessRemoveBlockPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(blockPos.X);
            bufferedStream.WriteInt32(blockPos.Y);
            bufferedStream.WriteInt32(blockPos.Z);
        }
    }
}
