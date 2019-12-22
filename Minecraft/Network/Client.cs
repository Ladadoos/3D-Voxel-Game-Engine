using System;
using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Client
    {
        private PacketFactory packetFactory = new PacketFactory();

        private string host;
        private int port;
        private bool isConnected;
        private TcpClient tcpClient;
        private NetworkStream netStream;
        private BinaryReader reader;
        private BinaryWriter writer;
        private NetBufferedStream bufferedStream;

        public bool ConnectWith(string host, int port)
        {
            this.host = host;
            this.port = port;

            try
            {
                tcpClient = new TcpClient(host, port);
            }catch(Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            netStream = tcpClient.GetStream();
            reader = new BinaryReader(netStream);
            writer = new BinaryWriter(netStream);
            bufferedStream = new NetBufferedStream(new BufferedStream(netStream));
            isConnected = true;
            Console.WriteLine("Connected to server");
            return true;
        }

        public void Stop()
        {
            isConnected = false;
            netStream.Close();
            tcpClient.Close();
        }

        public void Update(Game game)
        {
            if (!isConnected)
            {
                return;
            }
            while (netStream.DataAvailable)
            {
                Packet packet = packetFactory.ReadPacket(reader);
                Console.WriteLine("Received packet [" + packet.ToString() + "]");
                packet.Execute(game);
            }
        }

        public void SendPacket(Packet packet)
        {
            packet.WriteToStream(bufferedStream);
            bufferedStream.FlushToSocket();
        }
    }
}
