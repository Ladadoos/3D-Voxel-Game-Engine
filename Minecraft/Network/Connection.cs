using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Connection
    {
        public TcpClient client;
        public NetworkStream netStream;
        public BinaryReader reader;
        public NetBufferedStream bufferedStream;
        private PacketFactory packetFactory = new PacketFactory();

        public void Close()
        {       
            netStream.Close();
            client.Close();
        }

        public bool WritePacket(Packet packet)
        {
            packet.WriteToStream(bufferedStream);
            return bufferedStream.FlushToSocket();
        }

        public Packet ReadPacket()
        {
            return packetFactory.ReadPacket(this);
        }
    }
}
