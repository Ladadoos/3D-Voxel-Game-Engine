using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

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

        private object writePacketLock = new object();
        private object readPacketLock = new object();
        private Queue<Packet> toSendPackets = new Queue<Packet>();
        private Queue<Packet> toProcessPackets = new Queue<Packet>();

        private Thread packetTransferThread;

        public Client(Game game)
        {
            this.game = game;

            serverConnection = new Connection() { state = ConnectionState.Started };

            packetTransferThread = new Thread(HandlePacketCommunication);
            packetTransferThread.IsBackground = true;
            packetTransferThread.Start();
        }

        private void HandlePacketCommunication()
        {
            while (serverConnection.state == ConnectionState.Started) { }

            while (true)
            {
                Thread.Sleep(5);

                if (serverConnection.state == ConnectionState.Closed)
                {
                    break;
                }

                lock (readPacketLock)
                {
                    try
                    {
                        while (serverConnection.netStream.DataAvailable)
                        {
                            Packet packet = serverConnection.ReadPacket();
                            Logger.Packet("Client received packet " + packet.ToString());
                            toProcessPackets.Enqueue(packet);
                        }
                    } catch (Exception e)
                    {
                        Logger.Error("Failed reading packet: " + e.Message);
                        Stop();
                    }
                }

                lock (writePacketLock)
                {
                    while(toSendPackets.Count > 0)
                    {
                        Packet p = toSendPackets.Dequeue();
                        serverConnection.WritePacket(p);
                    }
                }
            }

            Logger.Info("Client packet communication thread terminated.");
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

        public void Update(float deltaTime)
        {
            if (serverConnection.state == ConnectionState.Closed)
            {
                return;
            }

            CheckForKeepAlive(deltaTime);

            lock (readPacketLock)
            {
                while (toProcessPackets.Count > 0)
                {
                    toProcessPackets.Dequeue().Process(serverConnection.netHandler);
                }
            }
        }

        private void CheckForKeepAlive(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime > timeoutInSeconds)
            {
                elapsedTime = 0;
                Logger.Packet("Keep alive sent.");
                WritePacket(new PlayerKeepAlivePacket());
            }
        }

        public void WritePacket(Packet packet)
        {
            if (serverConnection.state == ConnectionState.Closed)
            {
                return;
            }

            Logger.Packet("Client wrote packet " + packet.ToString());
            lock (writePacketLock)
            {
                toSendPackets.Enqueue(packet);
            }
        }
    }
}
