using OpenTK;
using System;

namespace Minecraft
{
    abstract class Session
    {
        private readonly PlayerSettings DefaultPlayerSettings = new PlayerSettings
        {
            ViewDistance = 8
        };

        public Player Player { get; private set; }
        public INetHandler NetHandler { get; private set; }
        public PlayerSettings PlayerSettings { get; private set; }
        public Connection Connection { get; private set; }

        private SessionState state;
        public SessionState State {
            get { return state; }
            set {
                if (state == value) return;
                state = value;
                OnStateChangedHandler?.Invoke(this);
            }
        }

        public delegate void OnStateChanged(Session session);
        public event OnStateChanged OnStateChangedHandler;

        public delegate void OnPlayerAssigned();
        public event OnPlayerAssigned OnPlayerAssignedHandler;

        protected Session(Connection connection, INetHandler netHandler)
        {
            this.Connection = connection;
            NetHandler = netHandler;
            Player = Player;

            PlayerSettings = DefaultPlayerSettings;
            State = SessionState.AwaitingAcceptance;
        }

        public void AssignPlayer(Player player)
        {
            Player = player;
            OnPlayerAssignedHandler?.Invoke();
        }

        public bool IsChunkVisible(Vector2 chunkPosition)
        {
            Vector2 playerChunkPos = World.GetChunkPosition(Player.Position.X, Player.Position.Z);
            int dx = (int)Math.Abs(chunkPosition.X - playerChunkPos.X);
            int dz = (int)Math.Abs(chunkPosition.Y - playerChunkPos.Y);
            return dx >= 0 && dx <= PlayerSettings.ViewDistance && dz >= 0 && dz <= PlayerSettings.ViewDistance;
        }

        public bool IsBlockPositionInViewRange(Vector3i blockPos)
        {
            return IsChunkVisible(World.GetChunkPosition(blockPos.X, blockPos.Z));
        }

        public bool NetDataAvailable() => Connection.NetStream.DataAvailable;

        public bool WritePacket(Packet packet)
        {
            if (State == SessionState.Closed)
            {
                Logger.Error("Trying to send packet " + packet.GetType() + " while connection closed");
                return false;
            }

            if (!Connection.WritePacket(packet))
            {
                State = SessionState.Closed;
                return false;
            }
            return true;
        }

        public Packet ReadPacket()
        {
            return Connection.ReadPacket(this);
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
