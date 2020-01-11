using System;

namespace Minecraft
{
    class WorldServer : World
    {
        public EntityIdTracker entityIdTracker = new EntityIdTracker();
        private WorldGenerator worldGenerator;

        public WorldServer(Game game) : base(game)
        {
            OnBlockPlacedHandler += OnBlockPlacedServer;
            OnBlockRemovedHandler += OnBlockRemovedServer;
            OnEntityDespawnedHandler += OnEntityDespawnedServer;

            worldGenerator = new WorldGenerator();
        }

        public void GenerateTestMap()
        {
            Logger.Info("Starting initial chunk generation.");
            DateTime start = DateTime.Now;
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Chunk chunk = worldGenerator.GenerateBlocksForChunkAt(x, y);
                    LoadChunk(chunk);
                }
            }
            TimeSpan now2 = DateTime.Now - start;
            Logger.Info("Finished generation initial chunks. Took " + now2);
        }

        private void OnEntityDespawnedServer(Entity entity)
        {
            entityIdTracker.ReleaseId(entity.id);

            if(entity is ServerPlayer)
            {
                game.server.BroadcastPacket(new PlayerLeavePacket(entity.id, LeaveReason.Leave, "disconnect"));
            }
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            if (!game.server.isOpen) return;
            if (game.mode == RunMode.Server)
            {
                game.server.BroadcastPacket(new PlaceBlockPacket(newState, blockPos));
            } else
            {
                game.server.BroadcastPacketExceptToHost(new PlaceBlockPacket(newState, blockPos));
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
                game.server.BroadcastPacketExceptToHost(new RemoveBlockPacket(blockPos));
            }
        }
    }
}
