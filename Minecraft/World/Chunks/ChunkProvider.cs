using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkProvider
    {
        private readonly ServerSession session;
        public HashSet<Vector2> CurrentlyVisibleChunks { get; private set;} = new HashSet<Vector2>();

        private readonly object chunkRetrievalLock = new object();
        private readonly Queue<GenerateChunkOutput> receivedChunkData = new Queue<GenerateChunkOutput>();
        private readonly HashSet<GenerateChunkRequestOutgoing> outgoingChunkRequests = new HashSet<GenerateChunkRequestOutgoing>(); 

        public ChunkProvider(ServerSession session)
        {
            this.session = session;

            session.OnPlayerAssignedHandler += OnPlayerAssigned;
        }

        ~ChunkProvider()
        {
            session.Player.OnChunkChangedHandler -= OnPlayerChunkChanged;
        }

        private void OnPlayerAssigned()
        {
            session.Player.OnChunkChangedHandler += OnPlayerChunkChanged;
        }

        private void OnPlayerChunkChanged(World world, Vector2 playerGridPos)
        {
            HashSet<Vector2> newVisibleChunks = GetCurrentlyVisibleChunks(playerGridPos);
            RequestToLoadNewlyVisibleChunks(world, newVisibleChunks);
            UnloadNoLongerVisibleChunks(world, newVisibleChunks);
            CurrentlyVisibleChunks = newVisibleChunks;
        }

        private HashSet<Vector2> GetCurrentlyVisibleChunks(Vector2 playerGridPosition)
        {
            HashSet<Vector2> visibleChunks = new HashSet<Vector2>();

            //The visible chunks are the chunks in a square with sides (view distance) * 2 + 1
            //where the center chunk is where the player is at
            int dist = session.PlayerSettings.ViewDistance;
            dist *= 2;
            dist = dist + 1;

            int x = 0;
            int z = 0;
            int dx = 0;
            int dy = -1;
            int t = dist;
            for(int i = 0; i < dist * dist; i++)
            {
                float halfDist = dist / 2.0F;
                if(-halfDist < x && x <= halfDist && -halfDist < z && z <= halfDist)
                {
                    visibleChunks.Add(playerGridPosition + new Vector2(x, z));
                }
                if(x == z || (x < 0 && x == -z) || (x > 0 && x == 1 - z))
                {
                    t = dx;
                    dx = -dy;
                    dy = t;
                }
                x += dx;
                z += dy;
            }

            return visibleChunks;
        }

        private void RequestToLoadNewlyVisibleChunks(World world, HashSet<Vector2> newVisibleChunks)
        {
            foreach (Vector2 newChunkGridPos in newVisibleChunks)
            {
                if (!CurrentlyVisibleChunks.Contains(newChunkGridPos))
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
                        if(world.game.Server.IsOpenToPublic)
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

                Vector2 chunkPosition = new Vector2(answer.chunk.GridX, answer.chunk.GridZ);
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
            foreach (Vector2 prevChunkGridPos in CurrentlyVisibleChunks)
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

                Vector2 chunkPosition = new Vector2(chunk.GridX, chunk.GridZ);
                if(CurrentlyVisibleChunks.Contains(chunkPosition))
                {
                    world.AddPlayerPresenceToChunk(chunk);
                    if(world.game.Server.IsOpenToPublic)
                    {
                        session.WritePacket(new ChunkDataPacket(chunk));
                    }
                }
            }
        }
    }
}
