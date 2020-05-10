namespace Minecraft
{
    class ChatPacket : Packet
    {
        public string Message { get; private set; }

        public ChatPacket(string message) : base(PacketType.Chat)
        {
            Message = message;
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(Message);
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChatPacket(this);
        }
    }
}
