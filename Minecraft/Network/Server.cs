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
        public List<Session> clients = new List<Session>();
        private Thread connectionsThread;
        private int port;
        private string address;
        private TcpListener tcpServer;
        private bool isRunning;

        public WorldServer world { get; private set; }
        private Game game;

        /// <summary> Returns true if the server is open to more connections than the host. </summary>
        public bool isOpen { get; private set; }

        private ServerSession host;

        private object newJoinsLock = new object();
        private Queue<TcpClient> joinQueue = new Queue<TcpClient>();
        private Queue<ServerSession> toRemoveClients = new Queue<ServerSession>();

        private Dictionary<ServerSession, Stopwatch> keepAlives = new Dictionary<ServerSession, Stopwatch>();

        public Server(Game game, bool isOpen)
        {
            this.game = game;
            this.isOpen = isOpen;
        }

        public bool IsHost(Session session)
        {
            return session == host;
        }

        public void Start(string address, int port)
        {
            this.address = address;
            this.port = port;

            world = new WorldServer(game);

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
            Logger.Info("Reset keep alive for " + session?.player.id);
            keepAliveWatch.Restart();
        }

        private void CheckForKeepAlive()
        {
            foreach(KeyValuePair<ServerSession, Stopwatch> client in keepAlives)
            {
                if(client.Value.ElapsedMilliseconds >= Client.KeepAliveTimeoutSeconds * 1000)
                {
                    Logger.Warn("Failed to keep connection with " + client.Key.player?.id);
                    client.Key.state = SessionState.Closed;
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
                TcpClient client = tcpServer.AcceptTcpClient();
                lock (newJoinsLock)
                {
                    joinQueue.Enqueue(client);
                }
                Logger.Info("Server accepted new client.");
            }

            Logger.Warn("Server is closing down. Closing connections to all clients.");
            clients.ForEach(c => c.Close());
            tcpServer.Stop();
            Logger.Info("Server closed.");
        }

        private void OnSessionStateChanged(Session serverSession)
        {
            ServerSession session = (ServerSession)serverSession;
            if (session.state == SessionState.Accepted)
            {

            } else if (session.state == SessionState.Closed)
            {
                Logger.Info("Connection closed with " + session?.player.id);
                toRemoveClients.Enqueue(session);
            }
        }

        private void HandleClientLeave()
        {
            while (toRemoveClients.Count > 0)
            {
                ServerSession session = toRemoveClients.Dequeue();
                clients.Remove(session);
                try
                {
                    session.Close();
                }catch(Exception e)
                {
                    Logger.Error("Closing client connection failed: " + e.Message);
                }
                keepAlives.Remove(session);

                if(session.player != null)
                {
                    world.DespawnEntity(session.player.id);
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
                        client = newClient,
                        netStream = stream,
                        reader = new BinaryReader(stream),
                        bufferedStream = new NetBufferedStream(new BufferedStream(stream)),
                    };
                    ServerNetHandler netHandler = new ServerNetHandler(game);
                    ServerSession session = new ServerSession(clientConnection, netHandler);
                    netHandler.AssignSession(session);
                    session.OnStateChangedHandler += OnSessionStateChanged;

                    if (game.mode == RunMode.ClientServer && clients.Count == 0)
                    {
                        host = session;
                    }
                    clients.Add(session);

                    Stopwatch timeoutWatch = new Stopwatch();
                    timeoutWatch.Start();
                    keepAlives.Add(session, timeoutWatch);
                }
            }
        }

        public void Update()
        {
            HandleClientJoin();
            HandleClientLeave();
            CheckForKeepAlive();

            //Check for console input in a non-blocking way
            if (Console.KeyAvailable)
            {
                string input = Console.ReadLine();
                BroadcastPacket(new ChatPacket(input));
            }

            foreach (ServerSession client in clients)
            {
                if (client.state == SessionState.Closed || !client.NetDataAvailable())
                {
                    continue;
                }

                Packet packet = client.ReadPacket();
                Logger.Packet("Server received packet " + packet.ToString());
                packet.Process(client.netHandler);
            }
        }

        public void BroadcastPacket(Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            clients.ForEach(c => c.WritePacket(packet));
        }

        public void BroadcastPacketExceptTo(Session session, Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            foreach (Session client in clients)
            {
                if (client == session)
                {
                    continue;
                }
                client.WritePacket(packet);
            }
        }

        public void BroadcastPacketExceptToHost(Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                if (clients[i] == host) continue;
                clients[i].WritePacket(packet);
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
