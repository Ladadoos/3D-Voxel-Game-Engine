namespace Minecraft
{
    abstract class Packet
    {
        protected PacketType type;

        public Packet(PacketType type)
        {
            this.type = type;
        }

        public void WriteToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32((int)type);
            ToStream(bufferedStream);
        }

        public abstract void Execute(Game game);

        protected abstract void ToStream(NetBufferedStream bufferedStream);
    }
}
