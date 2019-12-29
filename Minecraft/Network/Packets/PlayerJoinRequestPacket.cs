namespace Minecraft
{
    class PlayerJoinRequestPacket : Packet
    {
        public string name { get; private set; }

        public PlayerJoinRequestPacket(string name) : base(PacketType.PlayerJoinRequest)
        {
            this.name = name;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessJoinRequestPacket(this);
            //Logger.Info("Player " + name + " requested to join the world.");
            //game.localServer.Broadcast(new PlayerJoinAcceptPacket(name, 0));
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(name);
        }
    }
}
