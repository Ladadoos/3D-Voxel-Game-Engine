using OpenTK;
using System;

namespace Minecraft
{
    static class BlockPropagation
    {
        /// <summary>
        /// While propagating through blocks one at a time, it can be that a chunk local position is not chunk local anymore to the chunk it was in.
        /// The chunk reference should be changed and the position should be fixed accordingly.
        /// </summary>
        public static (Vector3i, Chunk) FixReference(World world, Vector3i position, Chunk chunk, out bool wasReferenceFixable)
        {
            if(position.X < -1 || position.X > 16 || position.Z < -1 || position.Z > 16)
                throw new Exception();

            wasReferenceFixable = true;
            if(position.X < 0 && position.Z < 0)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ - 1), out Chunk cXNegZNeg))
                {
                    chunk = cXNegZNeg;
                    position.X = 15;
                    position.Z = 15;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.X < 0 && position.Z > 15)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ + 1), out Chunk cXNegZPos))
                {
                    chunk = cXNegZPos;
                    position.X = 15;
                    position.Z = 0;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.X > 15 && position.Z > 15)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ + 1), out Chunk cXPosZPos))
                {
                    chunk = cXPosZPos;
                    position.X = 0;
                    position.Z = 0;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.X > 15 && position.Z < 0)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ - 1), out Chunk cXPosZNeg))
                {
                    chunk = cXPosZNeg;
                    position.X = 0;
                    position.Z = 15;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.X < 0)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg))
                {
                    chunk = cXNeg;
                    position.X = 15;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.X > 15)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos))
                {
                    chunk = cXPos;
                    position.X = 0;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.Z < 0)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg))
                {
                    chunk = cZNeg;
                    position.Z = 15;
                } else
                {
                    wasReferenceFixable = false;
                }
            } else if(position.Z > 15)
            {
                if(world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos))
                {
                    chunk = cZPos;
                    position.Z = 0;
                } else
                {
                    wasReferenceFixable = false;
                }
            }

            if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                wasReferenceFixable = false;
            return (position, chunk);
        }
    }
}
