using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    static class FloodFillLight
    {
        /// <summary>
        /// Struct used in the BFS algortihm used to propagate light
        /// </summary>
        struct LightAddNode
        {
            public readonly Chunk currentChunk;
            public readonly Vector3i currentChunkLocalPos;

            public LightAddNode(Chunk currentChunk, Vector3i currentChunkLocalPos)
            {
                this.currentChunk = currentChunk;
                this.currentChunkLocalPos = currentChunkLocalPos;
            }
        }

        /// <summary>
        /// Struct used in the BFS algorithm used to remove light 
        /// </summary>
        struct LightRemoveNode
        {
            public readonly Chunk currentChunk;
            public readonly Vector3i currentChunkLocalPos;
            public readonly uint currentLight;

            public LightRemoveNode(Chunk currentChunk, Vector3i currentChunkLocalPos, uint currentLight)
            {
                this.currentChunk = currentChunk;
                this.currentChunkLocalPos = currentChunkLocalPos;
                this.currentLight = currentLight;
            }
        }

        public static Chunk[] RepairLightGridBlockRemoved(World world, Chunk chunk, Vector3i blockPos)
        {
            Queue<LightRemoveNode> darknessPropagationQueue = new Queue<LightRemoveNode>();
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();

            Vector3i sourceChunkLocalPos = blockPos.ToChunkLocal();

            HashSet<Chunk> updatedChunks = new HashSet<Chunk>();

            //Run BFS on the different color channels to propagate light accordingly.
            //After having removed light, we might need to have to propagate light again from other light sources.
            foreach(LightChannel channel in LightUtils.BlockVisibileColorChannels)
            {
                uint currentLightValue = LightUtils.GetLightOfChannel(chunk, sourceChunkLocalPos, channel);
                darknessPropagationQueue.Enqueue(new LightRemoveNode(chunk, sourceChunkLocalPos, currentLightValue));
                LightUtils.SetLightOfChannel(chunk, sourceChunkLocalPos, channel, 0);

                foreach(Chunk updatedChunk in PropagateDarkness(world, chunk, darknessPropagationQueue, lightPropagationQueue, channel))
                {
                    if(!updatedChunks.Contains(updatedChunk))
                    {
                        updatedChunks.Add(updatedChunk);
                    }
                }
                foreach(Chunk updatedChunk in PropagateLight(world, chunk, lightPropagationQueue, channel))
                {
                    if(!updatedChunks.Contains(updatedChunk))
                    {
                        updatedChunks.Add(updatedChunk);
                    }
                }
            }

            return updatedChunks.ToArray();
        }

        /// <summary>
        /// Updates chunk lightmaps as a result of having placed a block. Returns the chunks whose lightmaps were updated.
        /// </summary>
        public static Chunk[] RepairLightGridBlockAdded(World world, Chunk chunk, Vector3i blockPos, BlockState blockState)
        {
            //If a non-lightsource block was placed, darker areas might have appeared so this only needs to be fixed.
            if(!(blockState is ILightSource lightSource))
            {
                return RepairLightGridBlockRemoved(world, chunk, blockPos);
            }

            HashSet<Chunk> updatedChunks = new HashSet<Chunk>();

            Vector3i sourceChunkLocalPos = blockPos.ToChunkLocal();

            //Run BFS on the different color channels to propagate light accordingly.
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();
            foreach(LightChannel channel in LightUtils.BlockVisibileColorChannels)
            {
                uint lightValue = LightUtils.GetChannelColor(lightSource, channel);
                LightUtils.SetLightOfChannel(chunk, sourceChunkLocalPos, channel, lightValue);
                lightPropagationQueue.Enqueue(new LightAddNode(chunk, sourceChunkLocalPos));

                foreach(Chunk updatedChunk in PropagateLight(world, chunk, lightPropagationQueue, channel))
                {
                    if(!updatedChunks.Contains(updatedChunk))
                    {
                        updatedChunks.Add(updatedChunk);
                    }
                }

                lightPropagationQueue.Clear();
            }

            return updatedChunks.ToArray();
        }

        private static HashSet<Chunk> PropagateDarkness(World world, Chunk chunk, Queue<LightRemoveNode> darkQueue,
            Queue<LightAddNode> lightQueue, LightChannel channel)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            //Propagate light via BFS
            while(darkQueue.Count != 0)
            {
                LightRemoveNode lightRemoveNode = darkQueue.Dequeue();
                Chunk parentChunk = lightRemoveNode.currentChunk;
                Chunk currentChunk = parentChunk;

                Vector3i[] neighbourPositions = lightRemoveNode.currentChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    currentChunk = parentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    }else if(position.X > 15)
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
                    }else if(position.Z > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ + 1), out Chunk cZPos))
                            continue;

                        currentChunk = cZPos;
                        position.Z = 0;
                    }

                    if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                        continue;

                    uint neighbourLight = LightUtils.GetLightOfChannel(currentChunk, position, channel);

                    // Any light that is darker than our current light, we remove it.
                    if(neighbourLight != 0 && neighbourLight < lightRemoveNode.currentLight)
                    {
                        LightUtils.SetLightOfChannel(currentChunk, position, channel, 0);
                        darkQueue.Enqueue(new LightRemoveNode(currentChunk, position, neighbourLight));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } //Make sure to propagate already existing light from other sources to the areas that are now dark.
                    else if(neighbourLight >= lightRemoveNode.currentLight) 
                    {
                        lightQueue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    }
                }
            }

            return processedChunks;
        }

        private static HashSet<Chunk> PropagateLight(World world, Chunk chunk, Queue<LightAddNode> queue, LightChannel channel)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            //Propagate light via BFS
            while(queue.Count != 0)
            {
                LightAddNode lightAddNode = queue.Dequeue();
                Chunk parentChunk = lightAddNode.currentChunk;
                Chunk currentChunk = parentChunk;
                
                uint currentLight = LightUtils.GetLightOfChannel(parentChunk, lightAddNode.currentChunkLocalPos, channel);
                if(currentLight <= 1)
                    continue;

                Vector3i[] neighbourPositions = lightAddNode.currentChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    currentChunk = parentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    }else if(position.X > 15)
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
                    } else if(position.Z > 15)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX, currentChunk.GridZ + 1), out Chunk cZPos))
                            continue;

                        currentChunk = cZPos;
                        position.Z = 0;
                    }

                    if(position.Y < 0 || position.Y >= Constants.MAX_BUILD_HEIGHT)
                        continue;

                    Vector3i worldPos = new Vector3i(position.X + currentChunk.GridX * 16, position.Y, position.Z + currentChunk.GridZ * 16);
                    if(world.GetBlockAt(worldPos).GetBlock().IsOpaque)
                        continue;

                    //If our neighbour is enough darker compared to the light in the current block, propagate this light - 1 to said block.
                    uint neighbourLight = LightUtils.GetLightOfChannel(currentChunk, position, channel);
                    if(neighbourLight < currentLight - 1)
                    {
                        LightUtils.SetLightOfChannel(currentChunk, position, channel, currentLight - 1);
                        queue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    }
                }
            }

            return processedChunks;
        }
    }
}

