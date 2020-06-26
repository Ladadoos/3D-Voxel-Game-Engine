using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    abstract class World
    {
        public Game Game { get; private set; }

        private const float secondsPerTick = 0.05F;
        private float elapsedMillisecondsSinceLastTick;
        public Environment Environment { get; private set; }

        private readonly Queue<Vector3i> toRemoveBlocks = new Queue<Vector3i>();
        private readonly Queue<Tuple<Vector3i, BlockState>> toAddBlocks = new Queue<Tuple<Vector3i, BlockState>>();
        private readonly Queue<Entity> toRemoveEntities = new Queue<Entity>();

        private Dictionary<Vector2, int> chunkPlayerPopulation = new Dictionary<Vector2, int>();

        public ReadOnlyDictionary<int, Entity> LoadedEntities { get; private set; }
        private Dictionary<int, Entity> loadedEntities = new Dictionary<int, Entity>();

        public ReadOnlyDictionary<Vector2, Chunk> LoadedChunks { get; private set; }
        private Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();

        public ObjectPool<Chunk> ChunkPool { get; private set; } = new ObjectPool<Chunk>(128);

        public delegate void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState);
        public event OnBlockRemoved OnBlockRemovedHandler;

        public delegate void OnChunkLoaded(World world, Chunk chunk);
        public event OnChunkLoaded OnChunkLoadedHandler;

        public delegate void OnChunkUnloaded(World world, Chunk chunk);
        public event OnChunkUnloaded OnChunkUnloadedHandler;

        public delegate void OnEntitySpawned(World world, Entity entity);
        public event OnEntitySpawned OnEntitySpawnedHandler;

        public delegate void OnEntityDespawned(World world, Entity entity);
        public event OnEntityDespawned OnEntityDespawnedHandler;

        protected World(Game game)
        {
            Game = game;

            LoadedEntities = new ReadOnlyDictionary<int, Entity>(loadedEntities);
            LoadedChunks = new ReadOnlyDictionary<Vector2, Chunk>(loadedChunks);

            Environment = new Environment(2400);
            Environment.CurrentTime = 1200;
            Environment.AmbientColor = new Vector3(0.075F, 0.075F, 0.095F);
        }

        public bool DespawnEntity(int entityId)
        {
            if(!loadedEntities.TryGetValue(entityId, out Entity despawnedEntity))
                throw new Exception("Despawning unit that is not alive with ID " + entityId);

            if (loadedEntities.Remove(entityId))
            {
                despawnedEntity.RaiseOnDespawned();
                OnEntityDespawnedHandler?.Invoke(this, despawnedEntity);
                return true;
            }
            return false;
        }

        public void SpawnEntity(Entity entity)
        {
            loadedEntities.Add(entity.ID, entity);
            OnEntitySpawnedHandler?.Invoke(this, entity);
        }

        public void AddPlayerPresenceToChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.GridX, chunk.GridZ);
            if(chunkPlayerPopulation.TryGetValue(chunkPos, out int population))
            {
                int newPopulation = population + 1;
                if((this is WorldServer && newPopulation > 2) && !Game.Server.IsOpenToPublic)
                    throw new ArgumentException("Private world server population should never exceed " + newPopulation);

                if((this is WorldClient && newPopulation > 1))
                    throw new ArgumentException("Client world server population should never exceed one. It had " + newPopulation);

                chunkPlayerPopulation[chunkPos] = newPopulation;
            } else
            {
                chunkPlayerPopulation.Add(chunkPos, 1);
                LoadChunk(chunk);
            }
        }

        public bool RemovePlayerPresenceOfChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.GridX, chunk.GridZ);
            if (chunkPlayerPopulation.TryGetValue(chunkPos, out int population))
            {
                int newPopulation = population - 1;
                if(newPopulation <= 0)
                {
                    chunkPlayerPopulation.Remove(chunkPos);
                    return UnloadChunk(chunk);
                }
                chunkPlayerPopulation[chunkPos] = newPopulation;
                return true;
            }
            Logger.Warn("Chunk with negative player population count: " + chunkPos);
            return false;
        }

        protected void LoadChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.GridX, chunk.GridZ);
            if (!loadedChunks.ContainsKey(chunkPos))
            {
                loadedChunks.Add(chunkPos, chunk);
                OnChunkLoadedHandler?.Invoke(this, chunk);
            } else
            {
                throw new Exception("World " + GetType() + " already had chunk data for " + chunkPos);
            }
        }

        protected bool UnloadChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.GridX, chunk.GridZ);
            if (loadedChunks.Remove(chunkPos))
            {
                OnChunkUnloadedHandler?.Invoke(this, chunk);
                OnChunkUnloadedPostProcess(chunk);
                return true;
            }
            return false;
        }

        protected virtual void OnChunkUnloadedPostProcess(Chunk chunk)
        {
            ChunkPool.ReturnObject(chunk);
        }

        public virtual void Update(float deltaTimeSeconds)
        {
            foreach(Entity entity in loadedEntities.Values)
            {
                entity.Update(deltaTimeSeconds, this);
            }

            Tick(deltaTimeSeconds);

            Environment.Update(deltaTimeSeconds);

            ClearBlockRemoveBuffer();
            ClearBlockAddBuffer();
            ClearEntityRemoveBuffer();
        }

        protected void ClearBlockRemoveBuffer()
        {
            while(toRemoveBlocks.Count > 0)
                RemoveBlockAt(toRemoveBlocks.Dequeue());
        }

        protected void ClearBlockAddBuffer()
        {
            while(toAddBlocks.Count > 0)
            {
                var toAddBlock = toAddBlocks.Dequeue();
                AddBlockToWorld(toAddBlock.Item1, toAddBlock.Item2);
            }
        }

        protected void ClearEntityRemoveBuffer()
        {
            while(toRemoveEntities.Count > 0)
            {
                loadedEntities.Remove(toRemoveEntities.Dequeue().ID);
            }
        }

        private void Tick(float deltaTime)
        {
            elapsedMillisecondsSinceLastTick += deltaTime;
            if (elapsedMillisecondsSinceLastTick < secondsPerTick)
            {
                return;
            }

            foreach(KeyValuePair<Vector2, Chunk> loadedChunk in loadedChunks)
            {
                loadedChunk.Value.Tick(elapsedMillisecondsSinceLastTick, this);
            }

            foreach(Entity entity in loadedEntities.Values)
            {
                entity.Tick(elapsedMillisecondsSinceLastTick, this);
            }

            elapsedMillisecondsSinceLastTick = 0;
        }

        public static Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            float x = worldX < 0 ? (float)Math.Floor((worldX) / 16) : worldX / 16;
            float z = worldZ < 0 ? (float)Math.Floor((worldZ) / 16) : worldZ / 16;
            return new Vector2((int)x, (int)z);
        }

        public void QueueToRemoveBlockAt(Vector3i blockPos)
        {
            toRemoveBlocks.Enqueue(blockPos);
        }

        public void QueueToRemoveBlocksAt(List<Vector3i> blockPositions)
        {
            foreach(Vector3i blockPos in blockPositions)
                toRemoveBlocks.Enqueue(blockPos);
        }

        public void QueueToRemoveBlocksAt(Vector3i[] blockPositions)
        {
            foreach(Vector3i blockPos in blockPositions)
                toRemoveBlocks.Enqueue(blockPos);
        }

        public void QueueToAddBlockAt(Vector3i blockPos, BlockState block)
        {
            toAddBlocks.Enqueue(new Tuple<Vector3i, BlockState>(blockPos, block));
        }

        private bool RemoveBlockAt(Vector3i blockPos)
        {
            if (IsOutsideBuildHeight(blockPos.Y))
            {
                Logger.Warn("Tried to remove block outside of building height.");
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(blockPos.X, blockPos.Z);
            if (!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                Logger.Warn("Tried to remove block in chunk that is not loaded.");
                return false;
            }

            Vector3i chunkLocalPos = blockPos.ToChunkLocal();

            BlockState oldState = GetBlockAt(blockPos);
            chunk.RemoveBlockAt(chunkLocalPos.X, chunkLocalPos.Y, chunkLocalPos.Z);
            oldState.GetBlock().OnDestroy(oldState, this, blockPos);
            OnBlockRemovedHandler?.Invoke(this, chunk, blockPos, oldState);
            return true;
        }

        private bool AddBlockToWorld(Vector3i blockPos, BlockState newBlockState)
        {
            BlockState oldState = GetBlockAt(blockPos);
            Vector2 chunkPos = GetChunkPosition(blockPos.X, blockPos.Z);
            bool blockPlacedInLoadedChunk = loadedChunks.TryGetValue(chunkPos, out Chunk chunk);

            if (!blockPlacedInLoadedChunk)
            {
                Logger.Warn("Tried to place block in chunk that is not loaded.");
                return false;
            }

            if (this is WorldServer)
            {
                if (newBlockState.GetBlock() == Blocks.Air)
                {
                    Logger.Warn("Tried to place air.");
                    return false;
                }

                if (IsOutsideBuildHeight(blockPos.Y))
                {
                    Logger.Warn("Tried to place block outside of building height.");
                    return false;
                }

                if(oldState.GetBlock() != Blocks.Air && !oldState.GetBlock().IsOverridable)
                {
                    Logger.Warn("Tried to place block where there was already one.");
                    return false;
                }

                foreach(Entity entity in loadedEntities.Values)
                {
                    if (newBlockState.GetBlock().GetCollisionBox(newBlockState, blockPos).Any(aabb => entity.Hitbox.Intersects(aabb)))
                    {
                        Console.WriteLine("Block tried to placed was in player");
                        return false;
                    }
                }
            }

            Vector3i chunkLocalPos = blockPos.ToChunkLocal();

            chunk.AddBlockAt(chunkLocalPos.X, chunkLocalPos.Y, chunkLocalPos.Z, newBlockState);
            newBlockState.GetBlock().OnAdd(newBlockState, this, blockPos);
            OnBlockPlacedHandler?.Invoke(this, chunk, blockPos, oldState, newBlockState);

            return true;
        }

        public bool IsOutsideBuildHeight(int worldY)
        {
            return worldY < 0 || worldY >= Constants.MAX_BUILD_HEIGHT;
        }
        
        public BlockState GetBlockAt(Vector3i blockPos)
        {
            Vector2 chunkPos = GetChunkPosition(blockPos.X, blockPos.Z);
            if(!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return Blocks.GetState(Blocks.Air);
            }

            return chunk.GetBlockAt(blockPos.ToChunkLocal());
        }

        public List<Chunk> GetCardinalChunks(Chunk chunk)
        {
            List<Chunk> chunks = new List<Chunk>();

            if(loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ - 1), out Chunk cXNegZNeg))
                chunks.Add(cXNegZNeg);

            if(loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ + 1), out Chunk cXNegZPos))
                chunks.Add(cXNegZPos);

            if(loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ + 1), out Chunk cXPosZPos))
                chunks.Add(cXPosZPos);

            if(loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ - 1), out Chunk cXPosZNeg))
                chunks.Add(cXPosZNeg);

            return chunks;
        }
    }
}
