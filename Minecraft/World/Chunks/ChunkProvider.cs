using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkProvider
    {
        private HashSet<Vector2> outgoingRequests = new HashSet<Vector2>();
        private ClientPlayer player;
        private Game game;
        private World world;
        private List<Chunk> toUnloadChunks = new List<Chunk>();

        private int viewDistance = 1;

        public ChunkProvider(Game game, World world)
        {
            this.game = game;
            this.world = world;
            player = game.player;
            
            world.OnChunkLoadedHandler += OnChunkLoaded;
            world.OnChunkUnloadedHandler += OnChunkUnloaded;
            game.player.OnChunkChangedHandler += OnPlayerChunkChanged;
        }

        public bool IsChunkRequestOutgoingFor(Vector2 chunkGridPosition)
        {
            return outgoingRequests.Contains(chunkGridPosition);
        }

        private bool IsGridPositionInViewDistanceOfPlayer(Vector2 gridPosition, Vector2 playerGridPosition)
        {
            float dx = Math.Abs(playerGridPosition.X - gridPosition.X);
            float dy = Math.Abs(playerGridPosition.Y - gridPosition.Y);
            return dx <= viewDistance && dy <= viewDistance;
        }

        private void OnPlayerChunkChanged(World world, Vector2 playerGridPos)
        {
            toUnloadChunks.Clear();
            foreach (KeyValuePair<Vector2, Chunk> gridChunk in world.loadedChunks)
            {
                if (!IsGridPositionInViewDistanceOfPlayer(gridChunk.Key, playerGridPos))
                {
                    toUnloadChunks.Add(gridChunk.Value);
                }
            }

            foreach(Chunk chunk in toUnloadChunks)
            {
                Vector2 chunkGridPosition = new Vector2(chunk.gridX, chunk.gridZ);

                world.RemovePlayerPresenceOfChunk(chunk);
                outgoingRequests.Remove(chunkGridPosition);
                game.client.WritePacket(new ChunkUnloadPacket(chunk.gridX, chunk.gridZ));
            }

            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector2 chunkPos = playerGridPos + new Vector2(x, z);

                    if (outgoingRequests.Contains(chunkPos))
                    {
                        continue;
                    }

                    if (!world.loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                    {
                        outgoingRequests.Add(chunkPos);
                        game.client.WritePacket(new ChunkDataRequestPacket((int)chunkPos.X, (int)chunkPos.Y));                      
                    }
                }
            }
        }

        private void OnChunkLoaded(Chunk chunk)
        {
            Vector2 chunkPosition = new Vector2(chunk.gridX, chunk.gridZ);
            outgoingRequests.Remove(chunkPosition);
        }

        private void OnChunkUnloaded(Chunk chunk)
        {
            Vector2 chunkPosition = new Vector2(chunk.gridX, chunk.gridZ);
            outgoingRequests.Remove(chunkPosition);
        }
    }
}
