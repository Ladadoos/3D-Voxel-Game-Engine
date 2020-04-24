using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkProvider
    {
        private ServerSession session;
        private PlayerSettings playerSettings;
        private HashSet<Vector2> currentlyVisibleChunks = new HashSet<Vector2>();

        private object chunkRetrievalLock = new object();
        private Queue<GenerateChunkOutput> receivedChunkData = new Queue<GenerateChunkOutput>();
        private HashSet<GenerateChunkRequestOutgoing> outgoingChunkRequests = new HashSet<GenerateChunkRequestOutgoing>(); 

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
            RequestToLoadNewlyVisibleChunks(world, newVisibleChunks);
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

        private void RequestToLoadNewlyVisibleChunks(World world, HashSet<Vector2> newVisibleChunks)
        {
            foreach (Vector2 newChunkGridPos in newVisibleChunks)
            {
                if (!currentlyVisibleChunks.Contains(newChunkGridPos))
                {
                    if(!world.loadedChunks.TryGetValue(newChunkGridPos, out Chunk chunk))
                    {
                        GenerateChunkRequestOutgoing request = new GenerateChunkRequestOutgoing
                        {
                            world = world,
                            gridPosition = newChunkGridPos
                        };

                        bool requested = false;
                        lock(chunkRetrievalLock)
                        {
                            if(!outgoingChunkRequests.Contains(request))
                            {
                                outgoingChunkRequests.Add(request);
                                requested = true;
                            }
                        }

                        if(requested)
                        {
                            ((WorldServer)world).RequestGenerationOfChunk(newChunkGridPos, ChunkRetrievedCallback);
                        }
                    } else
                    {
                        world.AddPlayerPresenceToChunk(chunk);
                        if(world.game.server.isOpen)
                        {
                            session.WritePacket(new ChunkDataPacket(chunk));
                        }
                    }
                }
            }
        }

        private void ChunkRetrievedCallback(GenerateChunkOutput answer)
        {
            lock(chunkRetrievalLock)
            {
                receivedChunkData.Enqueue(answer);

                Vector2 chunkPosition = new Vector2(answer.chunk.gridX, answer.chunk.gridZ);
                GenerateChunkRequestOutgoing request = new GenerateChunkRequestOutgoing
                {
                    world = answer.world,
                    gridPosition = chunkPosition
                };

                if(!outgoingChunkRequests.Remove(request))
                {
                    throw new InvalidOperationException("Removing entry for an outgoing request that isn't present!");
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
                    }
                    toUnloadChunks.Add(prevChunkGridPos);
                }
            }

            session.WritePacket(new ChunkUnloadPacket(toUnloadChunks));
        }

        public void Update()
        {
            bool hasEntry = false;
            GenerateChunkOutput outputEntry = new GenerateChunkOutput();
            lock(chunkRetrievalLock)
            {
                if(receivedChunkData.Count > 0)
                {
                    outputEntry = receivedChunkData.Dequeue();
                    hasEntry = true;
                }
            }

            if(hasEntry)
            {
                World world = outputEntry.world;
                Chunk chunk = outputEntry.chunk;

                Vector2 chunkPosition = new Vector2(chunk.gridX, chunk.gridZ);
                if(currentlyVisibleChunks.Contains(chunkPosition))
                {
                    world.AddPlayerPresenceToChunk(chunk);
                    if(world.game.server.isOpen)
                    {
                        session.WritePacket(new ChunkDataPacket(chunk));
                    }
                }
            }
        }
    }
}
