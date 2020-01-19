namespace Minecraft
{
    class ServerSession : Session
    {
        private ChunkProvider chunkProvider;

        public ServerSession(Connection connection, INetHandler netHandler)
            : base(connection, netHandler)
        {
            chunkProvider = new ChunkProvider(this, playerSettings);
        }
    }
}
