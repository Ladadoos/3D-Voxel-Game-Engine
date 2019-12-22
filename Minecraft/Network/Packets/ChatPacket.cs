using System;

namespace Minecraft
{
    class ChatPacket : Packet
    {
        private string message;

        public ChatPacket(string message) : base(PacketType.Chat)
        {
            this.message = message;
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteUtf8String(message);
        }

        public override void Execute(Game game)
        {
            Logger.Info("Received message: " + message);
        }
    }
}
