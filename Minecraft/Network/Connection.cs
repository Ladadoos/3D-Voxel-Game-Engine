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
        public INetHandler netHandler;
        public Player player;

        private ConnectionState _state;
        public ConnectionState state {
            get { return _state; }
            set {
                _state = value;
                OnStateChangedHandler?.Invoke(this);
            }
        }

        private PacketFactory packetFactory = new PacketFactory();

        public delegate void OnStateChanged(Connection connection);
        public event OnStateChanged OnStateChangedHandler;

        public void Close()
        {       
            netStream.Close();
            client.Close();
        }

        public void WritePacket(Packet packet)
        {
            if (state == ConnectionState.Closed)
            {
                Logger.Error("Trying to send packet " + packet.GetType() + " while connection closed");
                return;
            }

            packet.WriteToStream(bufferedStream);
            if (!bufferedStream.FlushToSocket())
            {
                state = ConnectionState.Closed;
            }
        }

        public Packet ReadPacket()
        {
            return packetFactory.ReadPacket(this);
        }
    }

    enum ConnectionState
    {
        Started,
        AwaitingAcceptance,
        Accepted, 
        Closed
    }
}
