using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Connection
    {
        public TcpClient Client { get; set; }
        public NetworkStream NetStream { get; set; }
        public BinaryReader Reader { get; set; }
        public NetBufferedStream Writer { get; set; }
        private readonly PacketFactory packetFactory = new PacketFactory();

        public void Close()
        {       
            NetStream.Close();
            Client.Close();
        }

        public bool WritePacket(Packet packet)
        {
            packet.WriteToStream(Writer);
            return Writer.FlushToSocket();
        }

        public Packet ReadPacket()
        {
            return packetFactory.ReadPacket(this);
        }
    }
}
