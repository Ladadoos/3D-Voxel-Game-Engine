namespace Minecraft
{
    class PlayerBlockInteractionPacket : Packet
    {
        public Vector3i BlockPos { get; private set; }

        public PlayerBlockInteractionPacket(Vector3i blockPos) : base(PacketType.PlayerBlockInteraction)
        {
            BlockPos = blockPos;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerBlockInteractionpacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteVector3i(BlockPos);
        }
    }
}
