using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    abstract class World
    {
        public readonly Game game;

        private const float secondsPerTick = 0.05F;
        private float elapsedMillisecondsSinceLastTick;
        public Environment Environment { get; private set; }

        private readonly Queue<List<Vector3i>> toRemoveBlocks = new Queue<List<Vector3i>>();
        private readonly Queue<Tuple<Vector3i, BlockState>> toAddBlocks = new Queue<Tuple<Vector3i, BlockState>>();
        private readonly Queue<Entity> toRemoveEntities = new Queue<Entity>();

        public Dictionary<int, Entity> loadedEntities { get; private set; } = new Dictionary<int, Entity>();
        public Dictionary<Vector2, Chunk> loadedChunks { get; private set; } = new Dictionary<Vector2, Chunk>();

        public Dictionary<Vector2, int> chunkPlayerPopulation { get; private set; } = new Dictionary<Vector2, int>();

        public delegate void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, int chainPos, int chainCount);
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
            this.game = game;
            Environment = new Environment(24);
            Environment.AmbientColor = new Vector3(0.025F, 0.025F, 0.025F);
        }

        public bool DespawnEntity(int entityId)
        {
            if(!loadedEntities.TryGetValue(entityId, out Entity despawnedEntity))
            {
                throw new Exception("Despawning unit that is not alive with ID " + entityId);
            }

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
                if(this is WorldClient && newPopulation > 1)
                    throw new ArgumentException("World client population should never exceed 1");
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
                return true;
            }
            return false;
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
            {
                List<Vector3i> blocks = toRemoveBlocks.Dequeue();
                for(int i = 0; i < blocks.Count; i++)
                {
                    RemoveBlockAt(blocks[i], i + 1, blocks.Count);
                }
            }
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
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public void QueueToRemoveBlockAt(Vector3i blockPos)
        {
            toRemoveBlocks.Enqueue(new List<Vector3i>() { blockPos });
        }

        public void QueueToRemoveBlocksAt(List<Vector3i> blockPositions)
        {
            toRemoveBlocks.Enqueue(blockPositions);
        }

        public void QueueToAddBlockAt(Vector3i blockPos, BlockState block)
        {
            toAddBlocks.Enqueue(new Tuple<Vector3i, BlockState>(blockPos, block));
        }

        private bool RemoveBlockAt(Vector3i blockPos, int chainPos, int chainCount)
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
            BlockState air = Blocks.Air.GetNewDefaultState();
            air.GetBlock().OnAdd(air, this, blockPos);
            OnBlockRemovedHandler?.Invoke(this, chunk, blockPos, oldState, chainPos, chainCount);
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
                return Blocks.Air.GetNewDefaultState();
            }

            return chunk.GetBlockAt(blockPos.ToChunkLocal());
        }
    }
}
