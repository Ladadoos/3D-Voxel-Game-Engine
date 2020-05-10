using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Minecraft
{
    class Client
    {
        private readonly Game game;

        private string host;
        private int port;
        private ClientSession session;

        public const float KeepAliveTimeoutSeconds = 35;
        private const float timeoutInSeconds = 30;
        private float elapsedTime;

        private readonly object  writePacketLock = new object();
        private readonly object readPacketLock = new object();
        private readonly Queue<Packet> toSendPackets = new Queue<Packet>();
        private readonly Queue<Packet> toProcessPackets = new Queue<Packet>();
        private readonly Thread packetTransferThread;

        public Client(Game game)
        {
            this.game = game;

            packetTransferThread = new Thread(HandlePacketCommunication);
            packetTransferThread.IsBackground = true;
            packetTransferThread.Start();
        }

        public PlayerSettings GetPlayerSettings() => session.PlayerSettings;

        private void HandlePacketCommunication()
        {
            while(session == null || session.State == SessionState.Started)
            {
                Thread.Sleep(5);
            }

            while (true)
            {
                Thread.Sleep(5);

                if (session.State == SessionState.Closed)
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
                Client = tcpClient,
                NetStream = netStream,
                Reader = new BinaryReader(netStream),
                Writer = new NetBufferedStream(new BufferedStream(netStream)),
            };
            ClientNetHandler netHandler = new ClientNetHandler(game);
            session = new ClientSession(serverConnection, netHandler);
            session.AssignPlayer(game.ClientPlayer);
            session.OnStateChangedHandler += OnStateChangedHandler;      
            netHandler.AssignSession(session);

            Logger.Info("Connected to server IP: " + host + " Port: " + port);
            WritePacket(new PlayerJoinRequestPacket("Player" + new Random().Next(100)));
            return true;
        }

        private void OnStateChangedHandler(Session session)
        {
            if(session.State == SessionState.Accepted)
            {
                Logger.Info("Client: server accepted my connection.");
            } else if (session.State == SessionState.Closed)
            {
                session.Close();
                Logger.Info("Client: my connection with server closed.");
            }
        }

        public void Stop()
        {
            session.State = SessionState.Closed;
        }

        public void Update(float deltaTime)
        {
            if (session.State == SessionState.Closed)
            {
                return;
            }

            CheckForKeepAlive(deltaTime);

            lock (readPacketLock)
            {
                while (toProcessPackets.Count > 0)
                {
                    toProcessPackets.Dequeue().Process(session.NetHandler);
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
