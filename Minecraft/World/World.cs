using System;
using System.Collections.Generic;

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

        public Vector2 GetChunkPosition(float xPos, float yPos)
        {
            return new Vector2((int)xPos >> 4, (int)yPos >> 4);
        }

        public bool AddBlockToWorld(int x, int y, int z, BlockType blockType)
        {
            if (IsOutsideBuildHeight(y))
            {
                return false;
            }

            Vector2 chunkPos = GetChunkPosition(x, z);
            Chunk chunk;
            if(!chunks.TryGetValue(chunkPos, out chunk))
            {
                Console.WriteLine("Tried to add block in junk that doesnt exist");
                return false;
            }

            if(x == Math.Floor(game.player.position.X) && 
                z == Math.Floor(game.player.position.Z) && 
                y == Math.Floor(game.player.position.Y))
            {
                Console.WriteLine("in player");
                return false;
            }

            if(blockType != BlockType.Air && GetBlockAt(x, y, z) != BlockType.Air)
            {
                return false;
            }

            int i = x & 15;
            int j = y;
            int k = z & 15;

            chunk.AddBlock(i, j, k, blockType);
     
            TryAddChunkToReprocessQueue(chunk);

            if (i == 0)
            {
                Chunk cXNeg = null;
                chunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out cXNeg);
                if (cXNeg != null)
                {
                    TryAddChunkToReprocessQueue(cXNeg);
                }
            }

            if (i == 15)
            {
                Chunk cXPos = null;
                chunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out cXPos);
                if (cXPos != null)
                {
                    TryAddChunkToReprocessQueue(cXPos);
                }
            }

            if (k == 0)
            {
                Chunk cZNeg = null;
                chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out cZNeg);
                if (cZNeg != null)
                {
                    TryAddChunkToReprocessQueue(cZNeg);
                }
            }

            if (k == 15)
            {
                Chunk cZPos = null;
                chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out cZPos);
                if (cZPos != null)
                {
                    TryAddChunkToReprocessQueue(cZPos);
                }
            }

            Console.WriteLine("Tried to add block in chunk" + i + "," + j + "," + k);
            return true;
        }

        private void TryAddChunkToReprocessQueue(Chunk toReprocessChunk)
        {
            if (!toReprocessChunks.Contains(toReprocessChunk))
            {
                toReprocessChunks.Add(toReprocessChunk);
            }
        }

        public bool IsOutsideBuildHeight(int height)
        {
            return height < 0 || height >= Constants.SECTIONS_IN_CHUNKS * Constants.SECTION_HEIGHT;
        }
        
        public BlockType GetBlockAt(int x, int y, int z)
        {
            if (IsOutsideBuildHeight(y))
            {
                return BlockType.Air;
            }

            Vector2 chunkPos = GetChunkPosition(x, z);
            Chunk chunk;
            if (!chunks.TryGetValue(chunkPos, out chunk))
            {
                return BlockType.Air;
            }

            int sectionHeight = y / Constants.SECTION_HEIGHT;
            if(chunk.sections[sectionHeight] == null)
            {
                return BlockType.Air;
            }

            int localX = x & 15;    
            int localY = y & 15;
            int localZ = z & 15;

            sbyte? blockType = chunk.sections[sectionHeight].blocks[localX, localY, localZ];
            if (blockType == null)
            {
                return BlockType.Air;
            }
            return (BlockType)blockType;
        }
    }
}
