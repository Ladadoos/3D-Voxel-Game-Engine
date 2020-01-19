using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Minecraft
{
    class Client
    {
        private Game game;

        private string host;
        private int port;
        private ClientSession session;

        public static readonly float KeepAliveTimeoutSeconds = 35;
        private static readonly float timeoutInSeconds = 30;
        private float elapsedTime;

        private object writePacketLock = new object();
        private object readPacketLock = new object();
        private Queue<Packet> toSendPackets = new Queue<Packet>();
        private Queue<Packet> toProcessPackets = new Queue<Packet>();

        private Thread packetTransferThread;

        public Client(Game game)
        {
            this.game = game;

            packetTransferThread = new Thread(HandlePacketCommunication);
            packetTransferThread.IsBackground = true;
            packetTransferThread.Start();
        }

        public PlayerSettings GetPlayerSettings() => session.playerSettings;

        private void HandlePacketCommunication()
        {
            while (session == null || session.state == SessionState.Started) { }

            while (true)
            {
                Thread.Sleep(5);

                if (session.state == SessionState.Closed)
                {
                    break;
                }

                lock (readPacketLock)
                {
                    try
                    {
                        while (session.NetDataAvailable())
                        {
                            Packet packet = session.ReadPacket();
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
                        Packet toSendPacket = toSendPackets.Dequeue();
                        Logger.Packet("Client wrote packet " + toSendPacket.ToString());
                        session.WritePacket(toSendPacket);
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
            Connection serverConnection = new Connection()
            {
                client = tcpClient,
                netStream = netStream,
                reader = new BinaryReader(netStream),
                bufferedStream = new NetBufferedStream(new BufferedStream(netStream)),
            };
            ClientNetHandler netHandler = new ClientNetHandler(game);
            session = new ClientSession(serverConnection, netHandler);
            session.AssignPlayer(game.player);
            session.OnStateChangedHandler += OnStateChangedHandler;      
            netHandler.AssignSession(session);

            Logger.Info("Connected to server IP: " + host + " Port: " + port);
            WritePacket(new PlayerJoinRequestPacket("Player" + new Random().Next(100)));
            return true;
        }

        private void OnStateChangedHandler(Session session)
        {
            if(session.state == SessionState.Accepted)
            {
                Logger.Info("Client: server accepted my connection.");
            } else if (session.state == SessionState.Closed)
            {
                session.Close();
                Logger.Info("Client: my connection with server closed.");
            }
        }

        public void Stop()
        {
            session.state = SessionState.Closed;
        }

        public void Update(float deltaTime)
        {
            if (session.state == SessionState.Closed)
            {
                return;
            }

            CheckForKeepAlive(deltaTime);

            lock (readPacketLock)
            {
                while (toProcessPackets.Count > 0)
                {
                    toProcessPackets.Dequeue().Process(session.netHandler);
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
            lock (writePacketLock)
            {
                toSendPackets.Enqueue(packet);
            }
        }
    }
}
