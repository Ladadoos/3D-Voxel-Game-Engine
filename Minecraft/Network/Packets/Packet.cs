namespace Minecraft
{
    abstract class Packet
    {
        protected PacketType type;

        protected Packet(PacketType type)
        {
            this.type = type;
        }

        public void WriteToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32((int)type);
            ToStream(bufferedStream);
        }

        public abstract void Process(INetHandler netHandler);

        protected abstract void ToStream(BufferedDataStream bufferedStream);
    }
}
