using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Connection
    {
        public TcpClient client;
        public NetworkStream netStream;
        public BinaryReader reader;
        public BinaryWriter writer;
        public NetBufferedStream bufferedStream;
        public INetHandler netHandler;

        private ConnectionState _state;
        public ConnectionState state {
            get { return _state; }
            set {
                _state = value;
                OnStateChangedHandler?.Invoke(this);
            }
        }

        public delegate void OnStateChanged(Connection connection);
        public event OnStateChanged OnStateChangedHandler;

        public void Close()
        {       
            netStream.Close();
            client.Close();
        }

        public void WritePacket(Packet packet)
        {
            packet.WriteToStream(bufferedStream);
            bufferedStream.FlushToSocket();
        }
    }

    enum ConnectionState
    {
        AwaitingAcceptance,
        Accepted, 
        Closed
    }
}
