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
        private List<Connection> clients = new List<Connection>();
        public Dictionary<Connection, ServerPlayer> players = new Dictionary<Connection, ServerPlayer>();
        private object clientsLock = new object();
        private Thread connectionsThread;
        private int port;
        private string address;
        private TcpListener tcpServer;
        private bool isRunning;

        public WorldServer world;
        private Game game;

        /// <summary> Returns true if the server is open to more connections than the host. </summary>
        public bool isOpen { get; private set; }

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

            connectionsThread = new Thread(ListenForConnections);
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

        private void ListenForConnections()
        {
            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpServer.Start();
            Logger.Info("Started listening for connections on " + GetType());
            isRunning = true;
            while (isRunning)
            {
                TcpClient client = tcpServer.AcceptTcpClient();
                Logger.Info("Server accepted new client.");
                NetworkStream stream = client.GetStream();
                Connection clientConnection = new Connection
                {
                    client = client,
                    netStream = stream,
                    reader = new BinaryReader(stream),
                    bufferedStream = new NetBufferedStream(new BufferedStream(stream)),
                    state = ConnectionState.AwaitingAcceptance
                };
                ServerNetHandler netHandler = new ServerNetHandler(game, clientConnection);
                clientConnection.netHandler = netHandler;
                clientConnection.OnStateChangedHandler += OnConnectionStateChanged;

                lock (clientsLock)
                {
                    clients.Add(clientConnection);
                }            
            }

            Logger.Warn("Server is closing down. Closing connections to all clients.");
            lock (clientsLock)
            {
                clients.ForEach(c => c.Close());
            }
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
            if(connection.state == ConnectionState.Accepted)
            {
                if (isOpen)
                {
                    Thread chunkThread = new Thread(ChunkSenderThread);
                    chunkThread.IsBackground = true;
                    chunkThread.Start(connection);

                    lock (clientsLock)
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            Connection client = clients[i];
                        }
                    }
                }
            }else if(connection.state == ConnectionState.Closed)
            {
                lock (clientsLock)
                {
                    clients.Remove(connection);
                    connection.Close();
                }
            }
        }

        public void Update()
        {
            //Check for console input in a non-blocking way
            if (Console.KeyAvailable)
            {
                string input = Console.ReadLine();
                BroadcastPacket(new ChatPacket(input));
            }

            lock (clientsLock)
            {
                for(int i = clients.Count - 1; i >= 0; i--)
                {
                    Connection client = clients[i];
                    if (client.state == ConnectionState.Closed || !client.netStream.DataAvailable)
                    {
                        continue;
                    }

                    Packet packet = client.ReadPacket();
                    Logger.Info("Server received packet " + packet.ToString());
                    packet.Process(client.netHandler);
                }
            }
        }

        public void BroadcastPacket(Packet packet)
        {
            lock (clientsLock)
            {
                Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
                clients.ForEach(c => c.WritePacket(packet));
            }
        }

        public void BroadcastPacketExceptTo(Connection connection, Packet packet)
        {
            lock (clientsLock)
            {
                Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    if (clients[i] == connection) continue;
                    clients[i].WritePacket(packet);
                }
            }
        }

        public void BroadcastPacketExcepToHost(Packet packet)
        {
            lock (clientsLock)
            {
                Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    if (clients[i] == clients[0]) continue;
                    clients[i].WritePacket(packet);
                }
            }
        }

        public void Stop()
        {
            isRunning = false;
        }
    }
}
