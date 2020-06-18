using OpenTK;

namespace Minecraft
{
    class BlockPropagation
    {
        private bool[] attemptBuffer = new bool[8];
        private Chunk[] chunkBuffer = new Chunk[8];

        public void Begin()
        {
            for(int i = 0; i < 8; i++)
            {
                attemptBuffer[i] = false;
                chunkBuffer[i] = null;
            }
        }

        private (Vector3i, Chunk) FixReferenceSide(World world, Vector3i position, Chunk chunk, int bufferIndex, 
            int x, int z, int cx, int cz, out bool wasReferenceFixable)
        {
            Vector2 chunkPos = new Vector2(chunk.GridX + cx, chunk.GridZ + cz);

            wasReferenceFixable = true;
            if(chunkBuffer[bufferIndex] == null && attemptBuffer[bufferIndex])
            {
                wasReferenceFixable = false;
            } else if(chunkBuffer[bufferIndex] != null)
            {
                chunk = chunkBuffer[bufferIndex];
                if(x != -1) position.X = x;
                if(z != -1) position.Z = z;
            } else if(world.loadedChunks.TryGetValue(chunkPos, out Chunk newChunk))
            {
                chunk = newChunk;
                if(x != -1) position.X = x;
                if(z != -1) position.Z = z;
                attemptBuffer[bufferIndex] = true;
                chunkBuffer[bufferIndex] = newChunk;
            } else
            {
                attemptBuffer[bufferIndex] = true;
                wasReferenceFixable = false;
            }
            return (position, chunk);
        }

        /// <summary>
        /// While propagating through blocks one at a time, it can be that a chunk local position is not chunk local anymore to the chunk it was in.
        /// The chunk reference should be changed and the position should be fixed accordingly.
        /// </summary>
        public (Vector3i, Chunk) FixReference(World world, Vector3i position, Chunk chunk, out bool wasReferenceFixable)
        {
            wasReferenceFixable = true;
            if(position.X < 0 && position.Z < 0)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 0, 15, 15, -1, -1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.X < 0 && position.Z > 15)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 1, 15, 0, -1, 1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.X > 15 && position.Z > 15)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 2, 0, 0, 1, 1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.X > 15 && position.Z < 0)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 3, 0, 15, 1, -1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.X < 0)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 4, 15, -1, -1, 0, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.X > 15)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 5, 0, -1, 1, 0, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.Z < 0)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 6, -1, 15, 0, -1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            } else if(position.Z > 15)
            {
                var (pPos, pChunk) = FixReferenceSide(world, position, chunk, 7, -1, 0, 0, 1, out bool wasFixed);
                wasReferenceFixable = wasFixed;
                if(wasFixed)
                {
                    position = pPos;
                    chunk = pChunk;
                }
            }

            if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                wasReferenceFixable = false;
            return (position, chunk);
        }
    }
}
