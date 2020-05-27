using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Minecraft
{
    class Server
    {
        public List<ServerSession> ConnectedClients { get; private set; } = new List<ServerSession>();
        private Thread connectionsThread;
        private int port;
        private string address;
        private TcpListener tcpServer;
        private bool isRunning;

        public WorldServer World { get; private set; }
        private readonly Game game;

        /// <summary> Returns true if the server is open to more connections than the host. </summary>
        public bool IsOpenToPublic { get; private set; }

        private ServerSession host;

        private readonly object newJoinsLock = new object();
        private readonly Queue<TcpClient> joinQueue = new Queue<TcpClient>();
        private readonly Queue<ServerSession> toRemoveClients = new Queue<ServerSession>();
        private readonly Dictionary<ServerSession, Stopwatch> keepAlives = new Dictionary<ServerSession, Stopwatch>();

        public Server(Game game, bool isOpen)
        {
            this.game = game;
            IsOpenToPublic = isOpen;
        }

        public bool IsHost(Session session)
        {
            return session == host;
        }

        public void Start(string address, int port)
        {
            this.address = address;
            this.port = port;

            World = new WorldServer(game);

            connectionsThread = new Thread(StartServerAndListenForConnections);
            connectionsThread.IsBackground = true;
            connectionsThread.Start();
        }

        public void UpdateKeepAliveFor(ServerSession session)
        {
            if(!keepAlives.TryGetValue(session, out Stopwatch keepAliveWatch))
            {
                Logger.Warn("Connection had no keep alive stopwatch assigned to it.");
                return;
            }
            Logger.Info("Reset keep alive for " + session?.Player.ID);
            keepAliveWatch.Restart();
        }

        private void CheckForKeepAlive()
        {
            foreach(KeyValuePair<ServerSession, Stopwatch> client in keepAlives)
            {
                if(client.Value.ElapsedMilliseconds >= Client.KeepAliveTimeoutSeconds * 1000)
                {
                    Logger.Warn("Failed to keep connection with " + client.Key.Player?.ID);
                    client.Key.State = SessionState.Closed;
                }
            }
        }

        private void StartServerAndListenForConnections()
        {
            tcpServer = new TcpListener(IPAddress.Any, port);
            tcpServer.Start();
            Logger.Info("Started listening for connections on " + GetType());

            isRunning = true;
            while (isRunning)
            {
                Thread.Sleep(100);
                TcpClient client = tcpServer.AcceptTcpClient();
                lock (newJoinsLock)
                {
                    joinQueue.Enqueue(client);
                }
                Logger.Info("Server accepted new client.");
            }

            Logger.Warn("Server is closing down. Closing connections to all clients.");
            ConnectedClients.ForEach(c => c.Close());
            tcpServer.Stop();
            Logger.Info("Server closed.");
        }

        private void OnSessionStateChanged(Session serverSession)
        {
            ServerSession session = (ServerSession)serverSession;
            if (session.State == SessionState.Closed)
            {
                Logger.Info("Connection closed with " + session?.Player.ID);
                toRemoveClients.Enqueue(session);
            }
        }

        private void HandleClientLeave()
        {
            while (toRemoveClients.Count > 0)
            {
                ServerSession session = toRemoveClients.Dequeue();
                ConnectedClients.Remove(session);
                try
                {
                    session.Close();
                }catch(Exception e)
                {
                    Logger.Error("Closing client connection failed: " + e.Message);
                }
                keepAlives.Remove(session);

                if(session.Player != null)
                {
                    World.DespawnEntity(session.Player.ID);
                }
            }
        }

        private void HandleClientJoin()
        {
            lock (newJoinsLock)
            {
                if (joinQueue.Count > 0)
                {
                    TcpClient newClient = joinQueue.Dequeue();
                    NetworkStream stream = newClient.GetStream();
                    Connection clientConnection = new Connection
                    {
                        Client = newClient,
                        NetStream = stream,
                        Reader = new BinaryReader(stream),
                        Writer = new BufferedDataStream(new BufferedStream(stream)),
                    };
                    ServerNetHandler netHandler = new ServerNetHandler(game);
                    ServerSession session = new ServerSession(clientConnection, netHandler);
                    netHandler.AssignSession(session);
                    session.OnStateChangedHandler += OnSessionStateChanged;

                    if (game.RunMode == RunMode.ClientServer && ConnectedClients.Count == 0)
                    {
                        host = session;
                    }
                    ConnectedClients.Add(session);

                    Stopwatch timeoutWatch = new Stopwatch();
                    timeoutWatch.Start();
                    keepAlives.Add(session, timeoutWatch);
                }
            }
        }

        public void Update(float deltaTimeSeconds)
        {
            HandleClientJoin();
            HandleClientLeave();
            CheckForKeepAlive();

            foreach (ServerSession client in ConnectedClients)
            {
                if (client.State == SessionState.Closed)
                {
                    continue;
                }

                client.Update(deltaTimeSeconds);

                if(!client.NetDataAvailable())
                {
                    continue;
                }

                Packet packet = client.ReadPacket();
                Logger.Packet("Server received packet " + packet.ToString());
                packet.Process(client.NetHandler);
            }
        }

        public void BroadcastPacket(Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            ConnectedClients.ForEach(c => c.WritePacket(packet));
        }

        public void BroadcastPacketExceptTo(Session session, Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            foreach (Session client in ConnectedClients)
            {
                if (client == session)
                {
                    continue;
                }
                client.WritePacket(packet);
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
