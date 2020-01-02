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

        private void OnBlockPlacedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            if (!game.server.isOpen) return;
            if(game.mode == RunMode.Server)
            {
                game.server.BroadcastPacket(new PlaceBlockPacket(newState, blockPos));
            } else
            {
                game.server.BroadcastPacketExceptTo(game.client.serverConnection, new PlaceBlockPacket(newState, blockPos));
            }

        }

        private void OnBlockRemovedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            if (!game.server.isOpen) return;
            if(game.mode == RunMode.Server)
            {
                game.server.BroadcastPacket(new RemoveBlockPacket(blockPos));
            } else
            {
                game.server.BroadcastPacketExceptTo(game.client.serverConnection, new RemoveBlockPacket(blockPos));
            }
        }
    }
}
