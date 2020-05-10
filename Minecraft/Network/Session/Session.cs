namespace Minecraft
{
    abstract class Session
    {
        private readonly PlayerSettings DefaultPlayerSettings = new PlayerSettings
        {
            ViewDistance = 2
        };

        public Player Player { get; private set; }
        public INetHandler NetHandler { get; private set; }
        public PlayerSettings PlayerSettings { get; private set; }
        private readonly Connection connection;

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
            this.connection = connection;
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

        public bool NetDataAvailable() => connection.NetStream.DataAvailable;

        public bool WritePacket(Packet packet)
        {
            if (State == SessionState.Closed)
            {
                Logger.Error("Trying to send packet " + packet.GetType() + " while connection closed");
                return false;
            }

            if (!connection.WritePacket(packet))
            {
                State = SessionState.Closed;
                return false;
            }
            return true;
        }

        public Packet ReadPacket()
        {
            return connection.ReadPacket();
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
