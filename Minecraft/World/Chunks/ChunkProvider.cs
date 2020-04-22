using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkProvider
    {
        private ServerSession session;
        private PlayerSettings playerSettings;
        private HashSet<Vector2> currentlyVisibleChunks = new HashSet<Vector2>();

        public ChunkProvider(ServerSession session, PlayerSettings playerSettings)
        {
            this.session = session;
            this.playerSettings = playerSettings;

            session.OnPlayerAssignedHandler += OnPlayerAssigned;
        }

        ~ChunkProvider()
        {
            session.player.OnChunkChangedHandler -= OnPlayerChunkChanged;
        }

        private void OnPlayerAssigned()
        {
            session.player.OnChunkChangedHandler += OnPlayerChunkChanged;
        }

        private void OnPlayerChunkChanged(World world, Vector2 playerGridPos)
        {
            HashSet<Vector2> newVisibleChunks = GetCurrentlyVisibleChunks(playerGridPos);
            LoadNewlyVisibleChunks(world, newVisibleChunks);
            UnloadNoLongerVisibleChunks(world, newVisibleChunks);
            currentlyVisibleChunks = newVisibleChunks;
        }

        private HashSet<Vector2> GetCurrentlyVisibleChunks(Vector2 playerGridPosition)
        {
            HashSet<Vector2> visibleChunks = new HashSet<Vector2>();

            int viewDistance = playerSettings.viewDistance;
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    visibleChunks.Add(playerGridPosition + new Vector2(x, z));
                }
            }

            return visibleChunks;
        }

        private void LoadNewlyVisibleChunks(World world, HashSet<Vector2> newVisibleChunks)
        {
            foreach (Vector2 newChunkGridPos in newVisibleChunks)
            {
                if (!currentlyVisibleChunks.Contains(newChunkGridPos))
                {
                    if (!world.loadedChunks.TryGetValue(newChunkGridPos, out Chunk chunk))
                    {
                        chunk = ((WorldServer)world).GenerateBlocksForChunk((int)newChunkGridPos.X, (int)newChunkGridPos.Y);
                    }
                    world.AddPlayerPresenceToChunk(chunk);

                    if (world.game.server.isOpen)
                    {
                        session.WritePacket(new ChunkDataPacket(chunk));
                    }
                }
            }
        }

        private void UnloadNoLongerVisibleChunks(World world, HashSet<Vector2> newVisibleChunks)
        {
            List<Vector2> toUnloadChunks = new List<Vector2>();
            foreach (Vector2 prevChunkGridPos in currentlyVisibleChunks)
            {
                if (!newVisibleChunks.Contains(prevChunkGridPos))
                {
                    if (world.loadedChunks.TryGetValue(prevChunkGridPos, out Chunk chunk))
                    {
                        world.RemovePlayerPresenceOfChunk(chunk);
                    } else
                    {
                        Logger.Error("This should never happen");
                    }
                    toUnloadChunks.Add(prevChunkGridPos);
                }
            }

            session.WritePacket(new ChunkUnloadPacket(toUnloadChunks));
        }
    }
}
