using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    static class FloodFillLight
    {
        public static Chunk[] RepairLightGridBlockRemoved(World world, Chunk chunk, Vector3i blockPos)
        {
            Queue<Tuple<Chunk, Vector3i, uint>> darknessPropagationQueue = new Queue<Tuple<Chunk, Vector3i, uint>>();
            Queue<Tuple<Chunk, Vector3i>> lightPropagationQueue = new Queue<Tuple<Chunk, Vector3i>>();

            Vector3i sourceChunkLocalPos = blockPos.ToChunkLocal();
            uint currentLightValue = chunk.LightMap.GetBlockLightAt(sourceChunkLocalPos);
            darknessPropagationQueue.Enqueue(new Tuple<Chunk, Vector3i, uint>(chunk, sourceChunkLocalPos, currentLightValue));
            chunk.LightMap.SetBlockLightAt(sourceChunkLocalPos, 0);
         
            Chunk[] updatedDarkChunks = PropagateDarkness(world, chunk, darknessPropagationQueue, lightPropagationQueue);
            Chunk[] updatedLightChunks = PropagateLight(world, chunk, lightPropagationQueue);

            return updatedDarkChunks.Union(updatedLightChunks).ToArray();
        }

        public static Chunk[] RepairLightGridBlockAdded(World world, Chunk chunk, Vector3i blockPos, BlockState blockState)
        {
            Vector3i sourceChunkLocalPos = blockPos.ToChunkLocal();
            if(blockState.GetBlock().LightIntensity == 0 && chunk.LightMap.GetBlockLightAt(sourceChunkLocalPos) == 0)
            {
                return new Chunk[0];
            }

            Queue<Tuple<Chunk, Vector3i>> lightPropagationQueue = new Queue<Tuple<Chunk, Vector3i>>();

            chunk.LightMap.SetBlockLightAt(sourceChunkLocalPos, blockState.GetBlock().LightIntensity);
            lightPropagationQueue.Enqueue(new Tuple<Chunk, Vector3i>(chunk, sourceChunkLocalPos));

            return PropagateLight(world, chunk, lightPropagationQueue);
        }

        private static Chunk[] PropagateDarkness(World world, Chunk chunk, Queue<Tuple<Chunk, Vector3i, uint>> darkQueue,
            Queue<Tuple<Chunk, Vector3i>> lightQueue)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            while(darkQueue.Count != 0)
            {
                Tuple<Chunk, Vector3i, uint> tup = darkQueue.Dequeue();
                Chunk parentChunk = tup.Item1;
                Chunk currentChunk = parentChunk;
                Vector3i chunkLocalPos = tup.Item2;
                uint currentLightValue = tup.Item3;

                Vector3i[] neighbourPositions = chunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    currentChunk = parentChunk;

                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    }
                    if(position.X > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX + 1, currentChunk.GridZ), out Chunk cXPos))
                            continue;

                        currentChunk = cXPos;
                        position.X = 0;
                    }
                    if(position.Z < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ - 1), out Chunk cZNeg))
                            continue;

                        currentChunk = cZNeg;
                        position.Z = 15;
                    }
                    if(position.Z > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ + 1), out Chunk cZPos))
                            continue;

                        currentChunk = cZPos;
                        position.Z = 0;
                    }
                    if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                    {
                        continue;
                    }

                    uint neighLightValue = currentChunk.LightMap.GetBlockLightAt(position);

                    if(neighLightValue != 0 && neighLightValue < currentLightValue)
                    {
                        currentChunk.LightMap.SetBlockLightAt(position, 0);
                        darkQueue.Enqueue(new Tuple<Chunk, Vector3i, uint>(currentChunk, position, neighLightValue));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } else if(neighLightValue >= currentLightValue)
                    {
                        lightQueue.Enqueue(new Tuple<Chunk, Vector3i>(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    }
                }
            }

            return processedChunks.ToArray();
        }

        private static Chunk[] PropagateLight(World world, Chunk chunk, Queue<Tuple<Chunk, Vector3i>> queue)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            while(queue.Count != 0)
            {
                Tuple<Chunk, Vector3i> tup = queue.Dequeue();
                Chunk parentChunk = tup.Item1;
                Chunk currentChunk = parentChunk;
                Vector3i chunkLocalPos = tup.Item2;

                uint lightValue = currentChunk.LightMap.GetBlockLightAt(chunkLocalPos);
                if(lightValue <= 1)
                {
                    continue;
                }

                Vector3i[] neighbourPositions = chunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    currentChunk = parentChunk;

                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    }
                    if(position.X > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX + 1, currentChunk.GridZ), out Chunk cXPos))
                            continue;

                        currentChunk = cXPos;
                        position.X = 0;
                    }
                    if(position.Z < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ - 1), out Chunk cZNeg))
                            continue;

                        currentChunk = cZNeg;
                        position.Z = 15;
                    }
                    if(position.Z > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ + 1), out Chunk cZPos))
                            continue;

                        currentChunk = cZPos;
                        position.Z = 0;
                    }
                    if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                    {
                        continue;
                    }

                    uint neighLightValue = currentChunk.LightMap.GetBlockLightAt(position);

                    if(lightValue == 0)
                        throw new Exception();

                    Vector3i worldPos = new Vector3i(position.X + currentChunk.GridX * 16, position.Y, position.Z + currentChunk.GridZ * 16);
                    bool canGoThrough = !world.GetBlockAt(worldPos).GetBlock().IsOpaque;
                    bool isLower = neighLightValue < lightValue - 1;

                    if(isLower)
                    {
                        currentChunk.LightMap.SetBlockLightAt(position, lightValue - 1);

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);

                        if(canGoThrough)
                        {
                            queue.Enqueue(new Tuple<Chunk, Vector3i>(currentChunk, position));
                        }
                    }
                }
            }

            return processedChunks.ToArray();
        }
    }
}

