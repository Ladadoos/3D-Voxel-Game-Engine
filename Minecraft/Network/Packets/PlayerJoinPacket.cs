namespace Minecraft
{
    class PlayerJoinPacket : Packet
    {
        public string name { get; private set; }
        public int playerId { get; private set; }

        public PlayerJoinPacket(string name, int playerId) : base(PacketType.PlayerJoin)
        {
            this.name = name;
            this.playerId = playerId;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerJoinPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(playerId);
            bufferedStream.WriteUtf8String(name);
        }
    }
}
