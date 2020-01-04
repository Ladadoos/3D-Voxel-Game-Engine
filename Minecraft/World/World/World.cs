using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    abstract class World
    {
        protected Game game;

        private float secondsPerTick = 0.05F;
        private float elapsedMillisecondsSinceLastTick;
        private List<Vector3i> toRemoveBlocks = new List<Vector3i>();

        public Dictionary<int, Entity> loadedEntities = new Dictionary<int, Entity>();
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();

        public delegate void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState);
        public event OnBlockRemoved OnBlockRemovedHandler;

        public delegate void OnChunkLoaded(Chunk chunk);
        public event OnChunkLoaded OnChunkLoadedHandler;

        public World(Game game)
        {
            this.game = game;
        }

        public void AssignChunkStorage(Dictionary<Vector2, Chunk> storage)
        {
            loadedChunks = storage;
        }

        public void LoadChunk(Chunk chunk)
        {
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if (!loadedChunks.ContainsKey(chunkPos))
            {
                loadedChunks.Add(chunkPos, chunk);
                OnChunkLoadedHandler?.Invoke(chunk);
            } else
            {
                //throw new Exception("Already had chunk data for " + chunkPos);
            }
        }

        public void Update(float deltaTime)
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
        }

        private void Tick(float deltaTime)
        {
            elapsedMillisecondsSinceLastTick += deltaTime;
            if (elapsedMillisecondsSinceLastTick > secondsPerTick)
            {
                foreach (KeyValuePair<Vector2, Chunk> loadedChunk in loadedChunks)
                {
                    loadedChunk.Value.Tick(this, elapsedMillisecondsSinceLastTick);
                }
                elapsedMillisecondsSinceLastTick = 0;
            }
        }

        //UPDATE
        public Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public void QueueToRemoveBlockAt(Vector3i blockPos)
        {
            toRemoveBlocks.Add(blockPos);
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

            BlockState oldState = GetBlockAt(blockPos);
            if (oldState.GetBlock() == Blocks.Air)
            {
                Logger.Warn("Tried to remove block where there was none.");
                return false;
            }

            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            int worldY = blockPos.Y;

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

                if (!blockPlacedInLoadedChunk)
                {
                    Logger.Warn("Tried to place block in chunk that is not loaded.");
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

            BlockState blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }
            return blockType;
        }
    }
}
