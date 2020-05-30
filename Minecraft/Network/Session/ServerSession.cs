namespace Minecraft
{
    class ServerSession : Session
    {
        private readonly ChunkProvider chunkProvider;

        public ServerSession(Connection connection, INetHandler netHandler)
            : base(connection, netHandler)
        {
            chunkProvider = new ChunkProvider(this);
        }

        public void Update(float deltaTimeSeconds)
        {
            chunkProvider.Update(deltaTimeSeconds);
        }
    }
}
