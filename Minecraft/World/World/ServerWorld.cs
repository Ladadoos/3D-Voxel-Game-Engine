namespace Minecraft
{
    class ServerWorld : World
    {
        public ServerWorld(Game game) : base(game)
        {
            OnBlockPlacedHandler += OnBlockPlacedServer;
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, BlockState oldState, BlockState newState)
        {
            game.localServer.Broadcast(game, new PlaceBlockPacket(newState));
        }
    }
}
