using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    class World
    {
        private Game game;

        public static int SeaLevel = 95;

        public WorldGenerator worldGenerator;

        public ChunkMeshGenerator chunkMeshGenerator;
        public Dictionary<Vector2, Chunk> chunks = new Dictionary<Vector2, Chunk>();
        public Dictionary<Vector2, RenderChunk> renderChunks = new Dictionary<Vector2, RenderChunk>();
        private List<Chunk> toReprocessChunks = new List<Chunk>();

        public World(Game game)
        {
            this.game = game;
            chunkMeshGenerator = new ChunkMeshGenerator(this);
            worldGenerator = new WorldGenerator();
        }

        public void CleanUp()
        {
            foreach (KeyValuePair<Vector2, RenderChunk> chunkToRender in renderChunks)
            {
                chunkToRender.Value.OnApplicationClosed();
            }
        }

        public void EndFrameUpdate()
        {
            for(int i = 0; i < toReprocessChunks.Count; i++)
            {
                chunkMeshGenerator.GenerateRenderMeshForChunk(toReprocessChunks[i]);
            }
            toReprocessChunks.Clear();
        }

        public void GenerateTestMap()
        {
            var start = DateTime.Now;
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                   Chunk chunk = worldGenerator.GenerateBlocksForChunkAt(x, y);
                   chunks.Add(new Vector2(x, y), chunk);
                   toReprocessChunks.Add(chunk);
                }
            }
            var now2 = DateTime.Now - start;
            Console.WriteLine("Generating init chunks took: " + now2 + " s");
        }

        public Vector2 GetChunkPosition(float worldX, float worldZ)
        {
            return new Vector2((int)worldX >> 4, (int)worldZ >> 4);
        }

        public bool AddBlockToWorld(Vector3 intPosition, BlockState blockstate)
        {
            return AddBlockToWorld((int)intPosition.X, (int)intPosition.Y, (int)intPosition.Z, blockstate);
        }

        public bool AddBlockToWorld(int worldX, int worldY, int worldZ, BlockState blockstate)
        {
            blockstate.position = new Vector3(worldX, worldY, worldZ);

            if (IsOutsideBuildHeight(worldY))
            {
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if(!chunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                Console.WriteLine("Tried to add block in junk that doesnt exist");
                return false;
            }

            //Small bug when: flying agaisnt a wall and breaking the block infront. It detects collision.
            if(blockstate.block.GetCollisionBox(blockstate).Any(aabb => game.player.hitbox.Intersects(aabb)))
            {
                Console.WriteLine("Block tried to placed was in player");
                return false;
            }

            if(blockstate.block != Block.Air && GetBlockAt(worldX, worldY, worldZ).block != Block.Air)
            {
                return false;
            }

            int localX = worldX & 15;
            int localZ = worldZ & 15;

            chunk.AddBlock(localX, worldY, localZ, blockstate);
            blockstate.block.OnAdded(blockstate, game);
     
            TryAddChunkToReprocessQueue(chunk);

            if (localX == 0)
            {
                chunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg);
                if (cXNeg != null)
                {
                    TryAddChunkToReprocessQueue(cXNeg);
                }
            }

            if (localX == 15)
            {
                chunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos);
                if (cXPos != null)
                {
                    TryAddChunkToReprocessQueue(cXPos);
                }
            }

            if (localZ == 0)
            {
                chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg);
                if (cZNeg != null)
                {
                    TryAddChunkToReprocessQueue(cZNeg);
                }
            }

            if (localZ == 15)
            {
                chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos);
                if (cZPos != null)
                {
                    TryAddChunkToReprocessQueue(cZPos);
                }
            }

            Console.WriteLine("Added block at " + worldX + "," + worldY + "," + worldZ);
            return true;
        }

        private void TryAddChunkToReprocessQueue(Chunk toReprocessChunk)
        {
            if (!toReprocessChunks.Contains(toReprocessChunk))
            {
                toReprocessChunks.Add(toReprocessChunk);
            }
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
                return Block.Air.GetNewDefaultState(); 
            }

            Vector2 chunkPos = GetChunkPosition(worldX, worldZ);
            if (!chunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                return Block.Air.GetNewDefaultState();
            }

            int sectionHeight = worldY / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return Block.Air.GetNewDefaultState(); 
            }

            int localX = worldX & 15;    
            int localY = worldY & 15;
            int localZ = worldZ & 15;

            BlockState blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return Block.Air.GetNewDefaultState(); 
            }
            return blockType;
        }
    }
}
