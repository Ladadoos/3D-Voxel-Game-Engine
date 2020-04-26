using OpenTK;
using System;

namespace Minecraft
{
    class WorldServer : World
    {
        private EntityIdTracker entityIdTracker = new EntityIdTracker();
        private WorldGenerator worldGenerator;

        public WorldServer(Game game) : base(game)
        {
            OnBlockPlacedHandler += OnBlockPlacedServer;
            OnBlockRemovedHandler += OnBlockRemovedServer;
            OnEntityDespawnedHandler += OnEntityDespawnedServer;

            worldGenerator = new WorldGenerator();
        }

        public int GenerateEntityId() => entityIdTracker.GenerateId();

        public void RequestGenerationOfChunk(Vector2 gridPosition, Action<GenerateChunkOutput> callback)
        {
            worldGenerator.AddChunkGenerationRequest(new GenerateChunkRequest()
            {
                gridPosition = gridPosition,
                world = this,
                callback = callback
            });
        }

        private void OnEntityDespawnedServer(World world, Entity entity)
        {
            entityIdTracker.ReleaseId(entity.id);

            if(entity is ServerPlayer)
            {
                game.server.BroadcastPacket(new PlayerLeavePacket(entity.id, LeaveReason.Leave, "disconnect"));
            }
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            foreach(ServerSession session in world.game.server.clients)
            {
                if(session.IsBlockPositionInViewRange(blockPos))
                {
                    session.WritePacket(new PlaceBlockPacket(newState, blockPos));
                }
            }           
        }

        private void OnBlockRemovedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            foreach(ServerSession session in world.game.server.clients)
            {
                if(session.IsBlockPositionInViewRange(blockPos))
                {
                    session.WritePacket(new RemoveBlockPacket(blockPos));
                }
            }
        }
    }
}
