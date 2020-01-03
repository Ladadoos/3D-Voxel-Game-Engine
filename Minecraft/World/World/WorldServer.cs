namespace Minecraft
{
    class WorldServer : World
    {
        public WorldServer(Game game) : base(game)
        {
            OnBlockPlacedHandler += OnBlockPlacedServer;
            OnBlockRemovedHandler += OnBlockRemovedServer;
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            if (!game.server.isOpen) return;
            if (game.mode == RunMode.Server)
            {
                game.server.BroadcastPacket(new PlaceBlockPacket(newState, blockPos));
            } else
            {
                game.server.BroadcastPacketExcepToHost(new PlaceBlockPacket(newState, blockPos));
            }
        }

        private void OnBlockRemovedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            if (!game.server.isOpen) return;
            if (game.mode == RunMode.Server)
            {
                game.server.BroadcastPacket(new RemoveBlockPacket(blockPos));
            } else
            {
                game.server.BroadcastPacketExcepToHost(new RemoveBlockPacket(blockPos));
            }
        }
    }
}
