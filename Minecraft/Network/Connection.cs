using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    struct Connection
    {
        public TcpClient client;
        public NetworkStream netStream;
        public BinaryReader reader;
        public BinaryWriter writer;
        public NetBufferedStream bufferedStream;
        public INetHandler netHandler;

        public void Close()
        {
            netStream.Close();
            client.Close();
        }

        public void SendPacket(Packet packet)
        {
            packet.WriteToStream(bufferedStream);
            bufferedStream.FlushToSocket();
        }
    }
}
