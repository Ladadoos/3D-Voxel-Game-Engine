using OpenTK;
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
            OnChunkLoadedHandler += OnChunkLoadedServer;

            worldGenerator = new WorldGenerator();
        }

        public Chunk GenerateBlocksForChunk(int gridX, int gridZ)
        {
            return worldGenerator.GenerateBlocksForChunkAt(gridX, gridZ);
        }

        public void GenerateSpawnArea()
        {
            Logger.Info("Starting initial chunk generation.");
            DateTime start = DateTime.Now;
            // No need for spawn area for now. Note that this needs to be changed with the unloading/loading
            // mechanism as described in ChunkProvider.cs and the population count of a chunk in World.cs
            for (int x = 0; x < 0; x++)
            {
                for (int y = 0; y < 0; y++)
                {
                    LoadChunk(GenerateBlocksForChunk(x, y));
                }
            }
            TimeSpan now2 = DateTime.Now - start;
            Logger.Info("Finished generation initial chunks. Took " + now2);
        }

        private void OnChunkLoadedServer(Chunk chunk)
        {
            if(game.mode == RunMode.ClientServer)
            {
                Vector2 chunkPosition = new Vector2(chunk.gridX, chunk.gridZ);
                if (game.world.chunkProvider.IsChunkRequestOutgoingFor(chunkPosition) || chunkPosition == Vector2.Zero)
                {
                    game.world.AddPlayerPresenceToChunk(chunk);
                }
            }
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
            game.server.BroadcastPacket(new PlaceBlockPacket(newState, blockPos));
        }

        private void OnBlockRemovedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            game.server.BroadcastPacket(new RemoveBlockPacket(blockPos));
        }
    }
}
