namespace Minecraft
{
    class PlayerJoinPacket : Packet
    {
        public string Name { get; private set; }
        public int PlayerID { get; private set; }

        public PlayerJoinPacket(string name, int playerId) : base(PacketType.PlayerJoin)
        {
            Name = name;
            PlayerID = playerId;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerJoinPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(PlayerID);
            bufferedStream.WriteUtf8String(Name);
        }
    }
}
