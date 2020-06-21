using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minecraft
{
    /*
     * Each player has their own chunk provider which is responsible 
     * for loading and unloading the chunks for each player accordingly. 
     * Chunk providers makes requests to for example the world generator
     * to generate chunks if these aren't already loaded.
     */
    class ChunkProvider
    {
        struct GenerateChunkRequestOutgoing
        {
            public Vector2 gridPosition; //At what chunk grid position we requested for a chunk
            public World world; //For what world we requested the chunk
        }

        /// <summary>
        /// The session this chunk provider is handling.
        /// </summary>
        private readonly ServerSession session;

        /// <summary>
        /// All chunk positions that are currently loaded for the player.
        /// </summary>
        public HashSet<Vector2> CurrentlyLoadedChunks { get; private set; } = new HashSet<Vector2>();

        /// <summary>
        /// Chunk data that has been received and is ready to be sent to the player.
        /// </summary>
        private readonly Queue<Tuple<Vector2, GenerateChunkOutput>> receivedChunkData = new Queue<Tuple<Vector2, GenerateChunkOutput>>();

        /// <summary>
        /// All the remaining chunk positions that this player will still ask the server to load.
        /// </summary>
        private Queue<Tuple<int, Vector2>> remainingChunkRequests = new Queue<Tuple<int, Vector2>>();

        /// <summary>
        /// All the outgoing chunk requests for chunks that were not loaded.
        /// </summary>
        private readonly HashSet<GenerateChunkRequestOutgoing> outgoingChunkRequests = new HashSet<GenerateChunkRequestOutgoing>();

        private readonly object chunkRetrievalLock = new object();
        private bool playedAssigned = false;

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
            playedAssigned = true;
            session.Player.OnChunkChangedHandler += OnPlayerChunkChanged;
        }

        private void OnPlayerChunkChanged(World world, Vector2 playerGridPos)
        {
            remainingChunkRequests = GetChunkLoadQueue(world, playerGridPos);

            //Unload all chunks that are outside of our view distance
            UnloadChunks(world, CurrentlyLoadedChunks.Where(c => !session.IsChunkVisible(c)).ToList());
        }

        private void UnloadChunks(World world, List<Vector2> chunkPositions)
        {
            foreach(Vector2 chunkPos in chunkPositions)
            {
                if(world.loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
                {
                    world.RemovePlayerPresenceOfChunk(chunk);
                } else
                {
                    throw new ArgumentException("Asked to unload chunk that was not loaded on the server!");
                }

                if(!CurrentlyLoadedChunks.Remove(chunkPos))
                    throw new ArgumentException("Trying to unload a player chunk that is not loaded!");
            }
            session.WritePacket(new ChunkUnloadPacket(chunkPositions));
        }

        /// <summary>
        /// Either directly sends the chunk data packet to the player if it is already present or 
        /// sends a request for generate the chunk at the given position.
        /// </summary>
        private void LoadChunk(World world, Vector2 chunkPos)
        {
            //If the seen chunk is not loaded, request generation of said chunk
            if(!world.loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                GenerateChunkRequestOutgoing request = new GenerateChunkRequestOutgoing
                {
                    world = world,
                    gridPosition = chunkPos
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
                    ((WorldServer)world).RequestGenerationOfChunk(session.Player.ID, chunkPos, ChunkRetrievedCallback);
                }
            } else
            {
                AddPresenceToChunkInWorld(world, chunk);
            }
        }

        private void AddPresenceToChunkInWorld(World world, Chunk chunk)
        {
            world.AddPlayerPresenceToChunk(chunk);
            if(world.game.Server.IsOpenToPublic)
            {
                CurrentlyLoadedChunks.Add(new Vector2(chunk.GridX, chunk.GridZ));
                session.WritePacket(new ChunkDataPacket(chunk));
            }
        }

        private Queue<Tuple<int, Vector2>> GetChunkLoadQueue(World world, Vector2 playerGridPosition)
        {
            Queue<Tuple<int, Vector2>> visibleChunks = new Queue<Tuple<int, Vector2>>();

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
                    Vector2 chunkPos = playerGridPosition + new Vector2(x, z);

                    //Don't load chunks that are already loaded
                    if(!CurrentlyLoadedChunks.Contains(chunkPos))
                    {
                        var request = new GenerateChunkRequestOutgoing() { world = world, gridPosition = chunkPos };
                        bool enqueueChunk = false;
                        lock(chunkRetrievalLock)
                        {
                            //Don't load chunks of which we already asked for them to be loaded and are awaiting for them
                            enqueueChunk = !outgoingChunkRequests.Contains(request) && 
                                //Don't load chunks that we have already received back but still need to process
                                receivedChunkData.Where(s => s.Item1 == chunkPos).Count() == 0;
                        }

                        if(enqueueChunk)
                        {
                            int maxChunkDistToPlayer = Math.Max(Math.Abs(x), Math.Abs(z));
                            visibleChunks.Enqueue(new Tuple<int, Vector2>(maxChunkDistToPlayer, chunkPos));
                        }
                    }
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

        private void ChunkRetrievedCallback(GenerateChunkOutput answer)
        {
            lock(chunkRetrievalLock)
            {
                receivedChunkData.Enqueue(new Tuple<Vector2, GenerateChunkOutput>(new Vector2(answer.chunk.GridX, answer.chunk.GridZ), answer));

                GenerateChunkRequestOutgoing request = new GenerateChunkRequestOutgoing
                {
                    world = answer.world,
                    gridPosition = new Vector2(answer.chunk.GridX, answer.chunk.GridZ)
                };

                if(!outgoingChunkRequests.Remove(request))
                {
                    throw new InvalidOperationException("Removing entry for an outgoing request that isn't present!");
                }
            }
        }

        public void Update(float deltaTimeSeconds)
        {
            bool foundEntry = false;
            GenerateChunkOutput outputEntry = new GenerateChunkOutput();
            lock(chunkRetrievalLock)
            {
                if(receivedChunkData.Count > 0)
                {
                    outputEntry = receivedChunkData.Dequeue().Item2;
                    foundEntry = true;
                }
            }

            if(foundEntry)
            {
                World world = outputEntry.world;
                Chunk chunk = outputEntry.chunk;

                if(session.IsChunkVisible(new Vector2(chunk.GridX, chunk.GridZ)))
                {
                    AddPresenceToChunkInWorld(world, chunk);
                } else
                {
                    Logger.Warn("Wasted chunk generation at chunk " + chunk.GridX + ", " + chunk.GridZ);
                }
            }

            if(playedAssigned)
            {
                Vector3 vel = session.Player.Velocity;
                int maxVel = (int)(vel.X * vel.X + vel.Z * vel.Z);
                const int topBoundary = 7225; //max distance sqrd
                int chunkDist = 0;
                if(maxVel < topBoundary)
                {
                    chunkDist = (int)Maths.ConvertRange(0, topBoundary, session.PlayerSettings.ViewDistance, 0, maxVel);
                }

                if(remainingChunkRequests.Count > 0 && remainingChunkRequests.Peek().Item1 <= chunkDist)
                {
                    LoadChunk(session.Player.World, remainingChunkRequests.Dequeue().Item2);
                }
            }
        }
    }
}
