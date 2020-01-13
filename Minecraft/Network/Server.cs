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
        public List<Connection> clients = new List<Connection>();
        private Thread connectionsThread;
        private int port;
        private string address;
        private TcpListener tcpServer;
        private bool isRunning;

        public WorldServer world { get; private set; }
        private Game game;

        /// <summary> Returns true if the server is open to more connections than the host. </summary>
        public bool isOpen { get; private set; }

        private object newJoinsLock = new object();
        private Queue<TcpClient> joinQueue = new Queue<TcpClient>();
        private Queue<Connection> toRemoveClients = new Queue<Connection>();

        private Dictionary<Connection, Stopwatch> keepAlives = new Dictionary<Connection, Stopwatch>();

        public Server(Game game, bool isOpen)
        {
            this.game = game;
            this.isOpen = isOpen;
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

        public void GenerateSpawnArea()
        {
            world.GenerateSpawnArea();
        }

        public void UpdateKeepAliveFor(Connection connection)
        {
            if(!keepAlives.TryGetValue(connection, out Stopwatch keepAliveWatch))
            {
                Logger.Warn("Connection had no keep alive stopwatch assigned to it.");
                return;
            }
            Logger.Info("Reset keep alive for " + connection?.player.id);
            keepAliveWatch.Restart();
        }

        private void CheckForKeepAlive()
        {
            foreach(KeyValuePair<Connection, Stopwatch> client in keepAlives)
            {
                if(client.Value.ElapsedMilliseconds >= Client.KeepAliveTimeoutSeconds * 1000)
                {
                    Logger.Warn("Failed to keep connection with " + client.Key.player?.id);
                    client.Key.state = ConnectionState.Closed;
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

        private void OnConnectionStateChanged(Connection connection)
        {
            if (connection.state == ConnectionState.Accepted)
            {

            } else if (connection.state == ConnectionState.Closed)
            {
                Logger.Info("Connection closed with " + connection?.player.id);
                toRemoveClients.Enqueue(connection);
            }
        }

        private void HandleClientLeave()
        {
            while (toRemoveClients.Count > 0)
            {
                Connection client = toRemoveClients.Dequeue();
                clients.Remove(client);
                try
                {
                    client.Close();
                }catch(Exception e)
                {
                    Logger.Error("Closing client connection: " + e.Message);
                }
                keepAlives.Remove(client);

                if(client.player != null)
                {
                    world.DespawnEntity(client.player.id);
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
                        state = ConnectionState.AwaitingAcceptance
                    };
                    ServerNetHandler netHandler = new ServerNetHandler(game, clientConnection);
                    clientConnection.netHandler = netHandler;
                    clientConnection.OnStateChangedHandler += OnConnectionStateChanged;
                    clients.Add(clientConnection);

                    Stopwatch timeoutWatch = new Stopwatch();
                    timeoutWatch.Start();
                    keepAlives.Add(clientConnection, timeoutWatch);
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

            foreach (Connection client in clients)
            {
                if (client.state == ConnectionState.Closed || !client.netStream.DataAvailable)
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

        public void BroadcastPacketExceptTo(Connection connection, Packet packet)
        {
            Logger.Packet("Server broadcasting packet [" + packet.GetType() + "]");
            foreach (Connection client in clients)
            {
                if (client == connection)
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
                if (clients[i] == clients[0]) continue;
                clients[i].WritePacket(packet);
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
