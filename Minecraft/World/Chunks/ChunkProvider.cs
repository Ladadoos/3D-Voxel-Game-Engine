using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkProvider
    {
        private HashSet<Vector2> outgoingRequests = new HashSet<Vector2>();
        private ClientPlayer player;
        private Game game;
        private World world;

        public ChunkProvider(Game game, World world)
        {
            this.game = game;
            this.world = world;
            player = game.player;
            
            world.OnChunkLoadedHandler += OnChunkLoaded;
        }

        public void OnChunkLoaded(Chunk chunk)
        {
            Vector2 chunkPosition = new Vector2(chunk.gridX, chunk.gridZ);
            outgoingRequests.Remove(chunkPosition);
        }

        public void CheckForNewChunks(World world)
        {
            int viewDistance = 2;
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    Vector2 chunkPos = world.GetChunkPosition(player.position.X, player.position.Z) + new Vector2(x, z);

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
    }
}
