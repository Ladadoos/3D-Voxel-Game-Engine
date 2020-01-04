using OpenTK;
using System;
using System.Collections.Generic;
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

        public WorldServer world;
        private Game game;

        /// <summary> Returns true if the server is open to more connections than the host. </summary>
        public bool isOpen { get; private set; }

        private object newJoinsLock = new object();
        private Queue<TcpClient> joinQueue = new Queue<TcpClient>();

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

        public void GenerateMap()
        {
            world.GenerateTestMap();
        }

        public Dictionary<Vector2, Chunk> GetChunKStorage()
        {
            return world.loadedChunks;
        }

        private void StartServerAndListenForConnections()
        {
            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
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

        private void ChunkSenderThread(object obj)
        {
            Connection connection = (Connection)obj;
            Logger.Info("Writing chunk data to stream.");
            foreach (KeyValuePair<Vector2, Chunk> kv in world.loadedChunks)
            {
                Logger.Info("Write chunk.");
                Thread.Sleep(500);
                connection.WritePacket(new ChunkDataPacket(kv.Value));
            }
        }

        private void OnConnectionStateChanged(Connection connection)
        {
            if (connection.state == ConnectionState.Accepted)
            {
                if (isOpen)
                {
                    Thread chunkThread = new Thread(ChunkSenderThread);
                    chunkThread.IsBackground = true;
                    chunkThread.Start(connection);
                }
            } else if (connection.state == ConnectionState.Closed)
            {
                clients.Remove(connection);
                connection.Close();
            }
        }

        public void Update()
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
                }
            }

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
                Logger.Info("Server received packet " + packet.ToString());
                packet.Process(client.netHandler);
            }
        }

        public void BroadcastPacket(Packet packet)
        {
            Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
            clients.ForEach(c => c.WritePacket(packet));
        }

        public void BroadcastPacketExceptTo(Connection connection, Packet packet)
        {
            Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
            foreach (Connection client in clients)
            {
                if (client == connection)
                {
                    continue;
                }
                client.WritePacket(packet);
            }
        }

        public void BroadcastPacketExcepToHost(Packet packet)
        {
            Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
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
