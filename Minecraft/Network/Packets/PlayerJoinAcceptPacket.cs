namespace Minecraft
{
    class PlayerJoinAcceptPacket : Packet
    {
        public string name { get; private set; }
        public int playerId { get; private set; }

        public PlayerJoinAcceptPacket(string name, int playerId) : base(PacketType.PlayerJoinAccept)
        {
            this.name = name;
            this.playerId = playerId;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinAcceptPacket(this);
            //Logger.Info("Player " + name + " joined the world with id " + playerId);
           // game.player = new ClientPlayer(game);
            //game.isReadyToPlay = true;
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(playerId);
            bufferedStream.WriteUtf8String(name);
        }
    }
}
