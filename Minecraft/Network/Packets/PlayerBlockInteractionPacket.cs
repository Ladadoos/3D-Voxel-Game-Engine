namespace Minecraft
{
    class PlayerBlockInteractionPacket : Packet
    {
        public Vector3i blockPos { get; private set; }

        public PlayerBlockInteractionPacket(Vector3i blockPos) : base(PacketType.PlayerBlockInteraction)
        {
            this.blockPos = blockPos;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerBlockInteractionpacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteFloat(blockPos.X);
            bufferedStream.WriteFloat(blockPos.Y);
            bufferedStream.WriteFloat(blockPos.Z);
        }
    }
}
