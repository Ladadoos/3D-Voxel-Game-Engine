namespace Minecraft
{
    class PlayerKickPacket : Packet
    {
        public KickReason reason { get; private set; }
        public string message { get; private set; }

        public PlayerKickPacket(KickReason reason, string message) : base(PacketType.PlayerKick)
        {
            this.reason = reason;
            this.message = message;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerKickPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32((int)reason);
            bufferedStream.WriteUtf8String(message);
        }
    }

    enum KickReason
    {
        Banned
    }
}
