namespace Minecraft
{
    class ServerWorldHook : IEventHook
    {
        private Game game;

        public ServerWorldHook(Game game)
        {
            this.game = game;
        }

        public void AddEventHooksFor(IEventAnnouncer obj)
        {
            World world = (World)obj;
            world.OnBlockPlacedHandler += OnBlockPlacedServer;
            world.OnBlockRemovedHandler += OnBlockRemovedServer;
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, BlockState oldState, BlockState newState)
        {
            if (game.server.isSingleplayer) return;
            game.server.BroadcastPacket(new PlaceBlockPacket(newState));
        }

        private void OnBlockRemovedServer(World world, Chunk chunk, BlockState oldState)
        {
            if (game.server.isSingleplayer) return;
            game.server.BroadcastPacket(new RemoveBlockPacket(oldState.position));
        }
    }
}
