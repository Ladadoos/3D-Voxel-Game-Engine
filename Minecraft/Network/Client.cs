using System;
using System.IO;
using System.Net.Sockets;

namespace Minecraft
{
    class Client
    {
        private string host;
        private int port;
        private Connection serverConnection;
        private Game game;

        public static readonly float KeepAliveTimeoutSeconds = 8;
        private static readonly float timeoutInSeconds = 5;
        private float elapsedTime;

        public Client(Game game)
        {
            this.game = game;

            serverConnection = new Connection()
            {
                state = ConnectionState.Closed
            };
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

            Logger.Info("Connected to server IP: " + host + " Port: " + port);
            WritePacket(new PlayerJoinRequestPacket("Player" + new Random().Next(100)));
            return true;
        }

        private void OnConnectionStateChanged(Connection connection)
        {
            if(connection.state == ConnectionState.Accepted)
            {
                Logger.Info("Client: server accepted my connection.");
            } else if (connection.state == ConnectionState.Closed)
            {
                serverConnection.Close();
                Logger.Info("Client: my connection with server closed.");
            }
        }

        public void Stop()
        {
            serverConnection.state = ConnectionState.Closed;
        }

        private void CheckForKeepAlive(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime > timeoutInSeconds)
            {
                elapsedTime = 0;
                Logger.Info("Keep alive sent.");
                serverConnection.WritePacket(new PlayerKeepAlivePacket());
            }
        }

        public void Update(float deltaTime)
        {
            if (serverConnection.state == ConnectionState.Closed)
            {
                return;
            }

            CheckForKeepAlive(deltaTime);

            try
            {
                while (serverConnection.netStream.DataAvailable)
                {
                    Packet packet = serverConnection.ReadPacket();
                    Logger.Packet("Client received packet " + packet.ToString());
                    packet.Process(serverConnection.netHandler);
                }
            }catch(Exception e)
            {
                Logger.Error("Failed reading packet: " + e.Message);
                Stop();
            }
        }

        public void WritePacket(Packet packet)
        {
            if (serverConnection.state == ConnectionState.Closed)
            {
                return;
            }
            Logger.Packet("Client wrote packet " + packet.ToString());
            serverConnection.WritePacket(packet);
        }
    }
}
