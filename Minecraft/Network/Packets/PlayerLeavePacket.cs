namespace Minecraft
{
    class PlayerLeavePacket : Packet
    {
        public int ID { get; private set; }
        public LeaveReason Reason { get; private set; }
        public string Message { get; private set; }

        public PlayerLeavePacket(int id, LeaveReason reason, string message) : base(PacketType.PlayerLeave)
        {
            ID = id;
            Reason = reason;
            Message = message;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlayerLeavePacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(ID);
            bufferedStream.WriteInt32((int)Reason);
            bufferedStream.WriteUtf8String(Message);
        }
    }

    enum LeaveReason
    {
        Banned,
        Leave
    }
}
