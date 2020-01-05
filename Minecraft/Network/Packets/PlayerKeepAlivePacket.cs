namespace Minecraft
{
    class PlayerKeepAlivePacket : Packet
    {
        public PlayerKeepAlivePacket() : base(PacketType.PlayerKeepAlive) { }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerKeepAlivePacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream) { }
    }
}
