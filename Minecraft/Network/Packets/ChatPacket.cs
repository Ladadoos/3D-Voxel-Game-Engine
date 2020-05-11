namespace Minecraft
{
    class ChatPacket : Packet
    {
        public string Sender { get; private set; }
        public string Message { get; private set; }

        public ChatPacket(string sender, string message) : base(PacketType.Chat)
        {
            Sender = sender;
            Message = message;
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(Sender);
            bufferedStream.WriteUtf8String(Message);
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChatPacket(this);
        }
    }
}
