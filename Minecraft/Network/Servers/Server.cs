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
        protected PacketFactory packetFactory = new PacketFactory();

        protected List<Connection> clients = new List<Connection>();
        protected object clientsLock = new object();
        protected Thread connectionsThread;
        protected int port;
        protected string address;
        protected TcpListener tcpServer;
        protected bool run;

        protected World world;
        protected Game game;

        public void Start(Game game, string address, int port)
        {
            this.game = game;
            this.address = address;
            this.port = port;

            InitializeWorld(game);
            world.GenerateTestMap();

            connectionsThread = new Thread(ListenForConnections);
            connectionsThread.IsBackground = true;
            connectionsThread.Start();
        }

        protected virtual void InitializeWorld(Game game)
        {
            world = new ServerWorld(game);
        }

        public World GetLocalWorld()
        {
            return world;
        }

        private void ListenForConnections()
        {
            tcpServer = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
            tcpServer.Start();
            Logger.Info("Started listening for connections on " + GetType());
            run = true;
            while (run)
            {
                TcpClient client = tcpServer.AcceptTcpClient();
                Logger.Info("Server accepted new client.");
                NetworkStream stream = client.GetStream();
                Connection connection = new Connection
                {
                    client = client,
                    stream = stream,
                    reader = new BinaryReader(stream),
                    writer = new BinaryWriter(stream),
                    bufferedStream = new NetBufferedStream(new BufferedStream(stream))
                };
                lock (clientsLock)
                {
                    clients.Add(connection);
                }
                Broadcast(null, new ChatPacket("New player joined!"));
                if(!(this is IntegratedServer))
                {
                    Logger.Info("Writing chunk data to stream.");
                    foreach (KeyValuePair<Vector2, Chunk> kv in world.loadedChunks)
                    {
                        new ChunkDataPacket(kv.Value).WriteToStream(connection.bufferedStream);
                    }
                    connection.bufferedStream.FlushToSocket();
                }
            }

            Logger.Warn("Server is closing down. Closing connections to all clients.");
            lock (clientsLock)
            {
                foreach (Connection conn in clients)
                {
                    conn.stream.Close();
                    conn.client.Close();
                }
            }
            tcpServer.Stop();
            Logger.Info("Server closed.");
        }

        public void Update(Game game)
        {
            //Check for console input in a non-blocking way
            if (Console.KeyAvailable)
            {
                string input = Console.ReadLine();
                Broadcast(null, new ChatPacket(input));
            }

            lock (clientsLock)
            {
                foreach (Connection conn in clients)
                {
                    if (!conn.stream.DataAvailable)
                    {
                        continue;
                    }

                    Packet packet = packetFactory.ReadPacket(conn.reader);
                    Logger.Info("Server received packet " + packet.ToString());
                    packet.Execute(game);
                }
            }
        }

        public void Broadcast(Game game, Packet packet)
        {
            lock (clientsLock)
            {
                Logger.Info("Server broadcasting packet [" + packet.GetType() + "]");
                foreach (Connection client in clients)
                {
                    packet.WriteToStream(client.bufferedStream);
                    client.bufferedStream.FlushToSocket();
                }
            }
        }
        
        public void Stop()
        {
            run = false;
        }
    }
}
