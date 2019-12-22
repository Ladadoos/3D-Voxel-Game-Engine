using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    struct Connection
    {
        public TcpClient client;
        public NetworkStream stream;
        public BinaryReader reader;
        public BinaryWriter writer;
        public NetBufferedStream bufferedStream;
    }
}
