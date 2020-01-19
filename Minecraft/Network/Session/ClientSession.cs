namespace Minecraft
{
    class ClientSession : Session
    {
        public ClientSession(Connection connection, INetHandler netHandler)
            : base(connection, netHandler)
        {
        }
    }
}
