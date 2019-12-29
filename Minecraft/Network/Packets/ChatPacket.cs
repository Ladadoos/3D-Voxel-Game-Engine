namespace Minecraft
{
    class ChatPacket : Packet
    {
        public string message { get; private set; }

        public ChatPacket(string message) : base(PacketType.Chat)
        {
            this.message = message;
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(message);
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChatPacket(this);
        }
    }
}
