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
        private Connection serverConnection;
        private Game game;

        public Client(Game game)
        {
            this.game = game;
        }

        public bool ConnectWith(string host, int port)
        {
            this.host = host;
            this.port = port;

            TcpClient tcpClient = null;
            try
            {
                tcpClient = new TcpClient(host, port);
            }catch(Exception e)
            {
                Logger.Error(e.ToString());
                return false;
            }

            NetworkStream netStream = tcpClient.GetStream();
            serverConnection = new Connection()
            {
                client = tcpClient,
                netStream = netStream,
                reader = new BinaryReader(netStream),
                writer = new BinaryWriter(netStream),
                bufferedStream = new NetBufferedStream(new BufferedStream(netStream))
            };
            ClientNetHandler netHandler = new ClientNetHandler(game, serverConnection);
            serverConnection.netHandler = netHandler;

            isConnected = true;
            Logger.Info("Connected to server IP: " + host + " Port: " + port);
            //SendPacket(new PlayerJoinRequestPacket("Player" + new Random().Next(100)));
            return true;
        }

        public void Stop()
        {
            isConnected = false;
            serverConnection.Close();
        }

        public void Update(Game game)
        {
            if (!isConnected)
            {
                return;
            }

            while (serverConnection.netStream.DataAvailable)
            {
                Packet packet = packetFactory.ReadPacket(serverConnection.reader);
                Logger.Info("Client received packet "+  packet.ToString());
                packet.Process(serverConnection.netHandler);
            }
        }

        public void SendPacket(Packet packet)
        {
            serverConnection.SendPacket(packet);
        }
    }
}
