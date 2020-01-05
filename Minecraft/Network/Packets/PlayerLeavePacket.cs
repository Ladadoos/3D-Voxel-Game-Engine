namespace Minecraft
{
    class PlayerLeavePacket : Packet
    {
        public int id { get; private set; }
        public LeaveReason reason { get; private set; }
        public string message { get; private set; }

        public PlayerLeavePacket(int id, LeaveReason reason, string message) : base(PacketType.PlayerLeave)
        {
            this.id = id;
            this.reason = reason;
            this.message = message;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerLeavePacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(id);
            bufferedStream.WriteInt32((int)reason);
            bufferedStream.WriteUtf8String(message);
        }
    }

    enum LeaveReason
    {
        Banned,
        Leave
    }
}
