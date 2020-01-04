using System;
using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Client
    {
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
                bufferedStream = new NetBufferedStream(new BufferedStream(netStream)),
                state = ConnectionState.AwaitingAcceptance
            };
            ClientNetHandler netHandler = new ClientNetHandler(game, serverConnection);
            serverConnection.player = game.player;
            serverConnection.netHandler = netHandler;
            serverConnection.OnStateChangedHandler += OnConnectionStateChanged;

            isConnected = true;
            Logger.Info("Connected to server IP: " + host + " Port: " + port);
            WritePacket(new PlayerJoinRequestPacket("Player" + new Random().Next(100)));
            return true;
        }

        private void OnConnectionStateChanged(Connection connection)
        {
            if(connection.state == ConnectionState.Accepted)
            {
                Logger.Info("Server accepted your connection.");
            } else if (connection.state == ConnectionState.Closed)
            {
                Logger.Info("Connection with server closed.");
                isConnected = false;
            }
        }

        public void Stop()
        {
            isConnected = false;
            serverConnection.Close();
        }

        public void Update()
        {
            if (!isConnected)
            {
                return;
            }

            while (serverConnection.netStream.DataAvailable)
            {
                Packet packet = serverConnection.ReadPacket();
                Logger.Info("Client received packet "+  packet.ToString());
                packet.Process(serverConnection.netHandler);
            }
        }

        public void WritePacket(Packet packet)
        {
            if (!isConnected) return;
            serverConnection.WritePacket(packet);
        }
    }
}
