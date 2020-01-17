using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    abstract class World
    {
        public Game game;

        private float secondsPerTick = 0.05F;
        private float elapsedMillisecondsSinceLastTick;

        private Queue<Vector3i> toRemoveBlocks = new Queue<Vector3i>();
        private Queue<Entity> toRemoveEntities = new Queue<Entity>();

        public Dictionary<int, Entity> loadedEntities = new Dictionary<int, Entity>();
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();

        public Dictionary<Vector2, int> chunkPlayerPopulation = new Dictionary<Vector2, int>();

        public delegate void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState);
        public event OnBlockRemoved OnBlockRemovedHandler;

        public delegate void OnChunkLoaded(Chunk chunk);
        public event OnChunkLoaded OnChunkLoadedHandler;

        public delegate void OnChunkUnloaded(Chunk chunk);
        public event OnChunkUnloaded OnChunkUnloadedHandler;

        public delegate void OnEntitySpawned(Entity entity);
        public event OnEntitySpawned OnEntitySpawnedHandler;

        public delegate void OnEntityDespawned(Entity entity);
        public event OnEntityDespawned OnEntityDespawnedHandler;

        public World(Game game)
        {
            this.game = game;
        }

        public bool DespawnEntity(int entityId)
        {
            if(!loadedEntities.TryGetValue(entityId, out Entity despawnedEntity))
            {
                return false;
            }

            if (loadedEntities.Remove(entityId))
            {
                despawnedEntity.RaiseOnDespawned();
                OnEntityDespawnedHandler?.Invoke(despawnedEntity);
                return true;
            }
            return false;
        }

        public void SpawnEntity(Entity entity)
        {
            loadedEntities.Add(entity.id, entity);
            OnEntitySpawnedHandler?.Invoke(entity);
        }

        public void AddPlayerPresenceToChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if(chunkPlayerPopulation.TryGetValue(chunkPos, out int population))
            {
                chunkPlayerPopulation[chunkPos] = population + 1;
            } else
            {
                chunkPlayerPopulation.Add(chunkPos, 1);
                LoadChunk(chunk);
            }
        }

        public bool RemovePlayerPresenceOfChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if (chunkPlayerPopulation.TryGetValue(chunkPos, out int population))
            {
                int newPopulation = population - 1;
                if(newPopulation == 0)
                {
                    chunkPlayerPopulation.Remove(chunkPos);
                    return UnloadChunk(chunk);
                }
                return true;
            }
            Logger.Warn("Chunk with negative player population count: " + chunkPos);
            return false;
        }

        protected void LoadChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if (!loadedChunks.ContainsKey(chunkPos))
            {
                loadedChunks.Add(chunkPos, chunk);
                OnChunkLoadedHandler?.Invoke(chunk);
            } else
            {
                throw new Exception("Already had chunk data for " + chunkPos);
            }
        }

        protected bool UnloadChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if (loadedChunks.Remove(chunkPos))
            {
                OnChunkUnloadedHandler?.Invoke(chunk);
                return true;
            }
            return false;
        }

        public virtual void Update(float deltaTime)
        {
            foreach(Entity entity in loadedEntities.Values)
            {
                entity.Update(deltaTime, this);
            }

            Tick(deltaTime);

            foreach(Vector3i toRemoveBlock in toRemoveBlocks)
            {
                RemoveBlockAt(toRemoveBlock);
            }
            toRemoveBlocks.Clear();

            while(toRemoveEntities.Count > 0)
            {
                loadedEntities.Remove(toRemoveEntities.Dequeue().id);
            }
        }

        private void Tick(float deltaTime)
        {
            elapsedMillisecondsSinceLastTick += deltaTime;
            if (elapsedMillisecondsSinceLastTick > secondsPerTick)
            {
                elapsedMillisecondsSinceLastTick = 0;

                foreach (KeyValuePair<Vector2, Chunk> loadedChunk in loadedChunks)
                {
                    loadedChunk.Value.Tick(this, elapsedMillisecondsSinceLastTick);
                }

                foreach (Entity entity in loadedEntities.Values)
                {
                    entity.Tick(deltaTime, this);
                }
            }
        }

        public Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public void QueueToRemoveBlockAt(Vector3i blockPos)
        {
            toRemoveBlocks.Enqueue(blockPos);
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

            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            int worldY = blockPos.Y;

            BlockState oldState = GetBlockAt(blockPos);
            BlockState air = Blocks.Air.GetNewDefaultState();
            chunk.AddBlock(localX, worldY, localZ, air);
            air.GetBlock().OnAdded(air, this);
            OnBlockRemovedHandler?.Invoke(this, chunk, blockPos, oldState);
            return true;
        }

        public bool AddBlockToWorld(Vector3i blockPos, BlockState newBlockState)
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

                if(oldState.GetBlock() != Blocks.Air)
                {
                    Logger.Warn("Tried to place block where there was already one.");
                    return false;
                }

                foreach(Entity entity in loadedEntities.Values)
                {
                    if (newBlockState.GetBlock().GetCollisionBox(newBlockState, blockPos).Any(aabb => entity.hitbox.Intersects(aabb)))
                    {
                        Console.WriteLine("Block tried to placed was in player");
                        return false;
                    }
                }
            }

            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            int worldY = blockPos.Y;
            chunk.AddBlock(localX, worldY, localZ, newBlockState);
            newBlockState.GetBlock().OnAdded(newBlockState, this);
            OnBlockPlacedHandler?.Invoke(this, chunk, blockPos, oldState, newBlockState);

            return true;
        }

        public bool IsOutsideBuildHeight(int worldY)
        {
            return worldY < 0 || worldY >= Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT;
        }
        
        public BlockState GetBlockAt(Vector3i blockPos)
        {
            if (IsOutsideBuildHeight(blockPos.Y))
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            Vector2 chunkPos = GetChunkPosition(blockPos.X, blockPos.Z);
            if (!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return Blocks.Air.GetNewDefaultState();
            }

            int sectionHeight = blockPos.Y / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            int localX = blockPos.X & 15;    
            int localY = blockPos.Y & 15;
            int localZ = blockPos.Z & 15;

            BlockState blockType = chunk.sections[sectionHeight].GetBlockAt(localX, localY, localZ);
            if (blockType == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }
            return blockType;
        }
    }
}
