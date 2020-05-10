namespace Minecraft
{
    class PlayerJoinRequestPacket : Packet
    {
        public string Name { get; private set; }

        public PlayerJoinRequestPacket(string name) : base(PacketType.PlayerJoinRequest)
        {
            Name = name;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinRequestPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(Name);
        }
    }
}
