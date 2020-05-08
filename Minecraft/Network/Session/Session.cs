namespace Minecraft
{
    abstract class Session
    {
        private readonly PlayerSettings DefaultPlayerSettings = new PlayerSettings
        {
            viewDistance = 2
        };

        public Player player { get; private set; }
        public INetHandler netHandler { get; private set; }
        private readonly Connection connection;
        public PlayerSettings playerSettings { get; private set; }

        public delegate void OnStateChanged(Session session);
        public event OnStateChanged OnStateChangedHandler;

        private SessionState _state;
        public SessionState state {
            get { return _state; }
            set {
                if (_state == value) return;
                _state = value;
                OnStateChangedHandler?.Invoke(this);
            }
        }

        public delegate void OnPlayerAssigned();
        public event OnPlayerAssigned OnPlayerAssignedHandler;

        protected Session(Connection connection, INetHandler netHandler)
        {
            this.connection = connection;
            this.netHandler = netHandler;
            this.player = player;

            playerSettings = DefaultPlayerSettings;
            state = SessionState.AwaitingAcceptance;
        }

        public void AssignPlayer(Player player)
        {
            this.player = player;
            OnPlayerAssignedHandler?.Invoke();
        }

        public bool NetDataAvailable() => connection.netStream.DataAvailable;

        public bool WritePacket(Packet packet)
        {
            if (state == SessionState.Closed)
            {
                Logger.Error("Trying to send packet " + packet.GetType() + " while connection closed");
                return false;
            }

            if (!connection.WritePacket(packet))
            {
                state = SessionState.Closed;
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
