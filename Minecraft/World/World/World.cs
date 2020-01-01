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
        protected List<Vector3i> toRemoveBlocks = new List<Vector3i>();

        public List<Entity> entities = new List<Entity>();

        public delegate void OnBlockPlaced(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState);
        public event OnBlockPlaced OnBlockPlacedHandler;

        public delegate void OnBlockRemoved(World world, Chunk chunk, Vector3i blockPos, BlockState oldState);
        public event OnBlockRemoved OnBlockRemovedHandler;

        public delegate void OnChunkLoaded(Chunk chunk);
        public event OnChunkLoaded OnChunkLoadedHandler;

        public World(Game game)
        {
            this.game = game;
            worldGenerator = new WorldGenerator();
            entities.Add(new Dummy(1));
        }

        public bool IsServer() => game.mode == RunMode.Server;

        public bool IsServerOpen()
        {
            if (game.server == null) throw new Exception("This should not be called.");
            return game.server.isOpen;
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

            foreach(Vector3i toRemoveBlock in toRemoveBlocks)
            {
                RemoveBlockAt(toRemoveBlock);
            }
            toRemoveBlocks.Clear();
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
                //Logger.Warn("Tried to remove block where there was none.");
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
            if(newBlockState.GetBlock() == Blocks.Air)
            {
                Logger.Warn("Tried to place air.");
                return false;
            }

            if (IsOutsideBuildHeight(blockPos.Y))
            {
                Logger.Warn("Tried to place block outside of building height.");
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(blockPos.X, blockPos.Z);
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

            BlockState oldState = GetBlockAt(blockPos);
            /*if (oldState.GetBlock() != Blocks.Air)
            {
                Logger.Warn("Tried to place block where there was already one.");
                return false;
            }*/

            int localX = blockPos.X & 15;
            int localZ = blockPos.Z & 15;
            int worldY = blockPos.Y;

            chunk.AddBlock(localX, worldY, localZ, newBlockState);
            newBlockState.GetBlock().OnAdded(newBlockState, this);
            OnBlockPlacedHandler?.Invoke(this, chunk, blockPos, oldState, newBlockState);

            //Console.WriteLine("Changed block at " + worldX + "," + worldY + "," + worldZ + " from " + oldState.block.GetType() + " to " + newBlockState.block.GetType());
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
