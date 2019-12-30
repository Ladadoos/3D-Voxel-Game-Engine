using System;
using System.Collections.Generic;
using OpenTK;

namespace Minecraft
{
    class World : IEventAnnouncer
    {
        public static int SeaLevel = 95;

        protected WorldGenerator worldGenerator;
        public Dictionary<Vector2, Chunk> loadedChunks = new Dictionary<Vector2, Chunk>();
        protected Game game;

        protected float secondsPerTick = 0.05F;
        protected float elapsedMillisecondsSinceLastTick;
        protected List<BlockState> toRemoveBlocks = new List<BlockState>();

        public List<Entity> entities = new List<Entity>();

        public delegate void OnBlockPlaced(World world, Chunk chunk, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, BlockState oldState);
        public event OnBlockRemoved OnBlockRemovedHandler;

        public delegate void OnChunkLoaded(Chunk chunk);
        public event OnChunkLoaded OnChunkLoadedHandler;

        public World(Game game)
        {
            this.game = game;
            worldGenerator = new WorldGenerator();
            //entities.Add(new Dummy(1));
        }

        public void AddEventHooks(IEventHook hook)
        {
            hook.AddEventHooksFor(this);
        }

        public void GenerateTestMap()
        {
            Logger.Info("Starting initial chunk generation.");
            var start = DateTime.Now;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                   Chunk chunk = worldGenerator.GenerateBlocksForChunkAt(x, y);
                   LoadChunk(chunk);
                }
            }
            var now2 = DateTime.Now - start;
            Logger.Info("Finished generation initial chunks. Took " + now2);
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

        public void Tick(float deltaTime)
        {
            elapsedMillisecondsSinceLastTick += deltaTime;
            if(elapsedMillisecondsSinceLastTick > secondsPerTick)
            {
                foreach (KeyValuePair<Vector2, Chunk> loadedChunk in loadedChunks)
                {
                    loadedChunk.Value.Tick(this, elapsedMillisecondsSinceLastTick);
                }
                elapsedMillisecondsSinceLastTick = 0;
            }

            foreach(BlockState toRemoveBlock in toRemoveBlocks)
            {
                RemoveBlockAt(toRemoveBlock.position);
            }
            toRemoveBlocks.Clear();
        }

        public Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public void QueueToRemoveBlockAt(Vector3 intPosition)
        {
            toRemoveBlocks.Add(GetBlockAt(intPosition));
        }

        private bool RemoveBlockAt(Vector3 intPosition)
        {
            int worldX = (int)intPosition.X;
            int worldY = (int)intPosition.Y;
            int worldZ = (int)intPosition.Z;

            if (IsOutsideBuildHeight(worldY))
            {
                Logger.Warn("Tried to remove block outside of building height.");
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if (!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                Logger.Warn("Tried to remove block in chunk that is not loaded.");
                return false;
            }

            BlockState oldState = GetBlockAt(worldX, worldY, worldZ);
            if (oldState.GetBlock() == Blocks.Air)
            {
                Logger.Warn("Tried to remove block where there was none.");
                return false;
            }

            int localX = worldX & 15;
            int localZ = worldZ & 15;

            BlockState air = Blocks.Air.GetNewDefaultState();
            air.position = intPosition;
            chunk.AddBlock(localX, worldY, localZ, air);
            air.GetBlock().OnAdded(air, this);
            OnBlockRemovedHandler?.Invoke(this, chunk, oldState);
            return true;
        }

        public bool AddBlockToWorld(Vector3 intPosition, BlockState blockstate)
        {
            return AddBlockToWorld((int)intPosition.X, (int)intPosition.Y, (int)intPosition.Z, blockstate);
        }

        public bool AddBlockToWorld(int worldX, int worldY, int worldZ, BlockState newBlockState)
        {
            if(newBlockState.GetBlock() == Blocks.Air)
            {
                Logger.Warn("Tried to place air.");
                return false;
            }

            //Dont like this whole setting position part like this....
            newBlockState.position = new Vector3(worldX, worldY, worldZ);

            if (IsOutsideBuildHeight(worldY))
            {
                Logger.Warn("Tried to place block outside of building height.");
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if(!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                Logger.Warn("Tried to place block in chunk that is not loaded.");
                return false;
            }

            /*if(newBlockState.block.GetCollisionBox(newBlockState).Any(aabb => game.player.hitbox.Intersects(aabb)))
            {
                Console.WriteLine("Block tried to placed was in player");
                return false;
            }*/

            BlockState oldState = GetBlockAt(worldX, worldY, worldZ);
            if (oldState.GetBlock() != Blocks.Air)
            {
                Logger.Warn("Tried to place block where there was already one.");
                return false;
            }

            int localX = worldX & 15;
            int localZ = worldZ & 15;

            chunk.AddBlock(localX, worldY, localZ, newBlockState);
            newBlockState.GetBlock().OnAdded(newBlockState, this);
            OnBlockPlacedHandler?.Invoke(this, chunk, oldState, newBlockState);

            //Console.WriteLine("Changed block at " + worldX + "," + worldY + "," + worldZ + " from " + oldState.block.GetType() + " to " + newBlockState.block.GetType());
            return true;
        }

        public bool IsOutsideBuildHeight(int worldY)
        {
            return worldY < 0 || worldY >= Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT;
        }
        
        public BlockState GetBlockAt(Vector3 worldIntPosition)
        {
            return GetBlockAt((int)worldIntPosition.X, (int)worldIntPosition.Y, (int)worldIntPosition.Z);
        }

        public BlockState GetBlockAt(int worldX, int worldY, int worldZ)
        {
            if (IsOutsideBuildHeight(worldY))
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if (!loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return Blocks.Air.GetNewDefaultState();
            }

            int sectionHeight = worldY / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }

            int localX = worldX & 15;    
            int localY = worldY & 15;
            int localZ = worldZ & 15;

            BlockState blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return Blocks.Air.GetNewDefaultState(); 
            }
            return blockType;
        }
    }
}
