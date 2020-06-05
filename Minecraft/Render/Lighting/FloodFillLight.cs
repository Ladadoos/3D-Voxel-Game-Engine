using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minecraft
{
    static class FloodFillLight
    {
        /// <summary>
        /// Struct used in the BFS algorithm used to propagate light
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

        public static Chunk[] RepairSunlightGridBlockRemoved(World world, Chunk chunk, Vector3i blockPos)
        {
            return RepairSunlightGridOnBlockAdded(world, chunk, blockPos, null);
        }

        public static Chunk[] RepairSunlightGrid(World world, Chunk chunk)
        {
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();
            HashSet<Chunk> updatedChunks = new HashSet<Chunk>();

            uint lowestEmptyHeight = chunk.GetLowestEmptySectionAfterEachOtherFromTop() * 16;

            if(lowestEmptyHeight == 15)
            {
                for(int x = 0; x < 16; x++)
                {
                    for(int z = 0; z < 16; z++)
                    {
                        if(!chunk.GetBlockAt(x, 255, z).GetBlock().IsOpaque)
                        {
                            Vector3i chunkLocalPos = new Vector3i(x, 255, z).ToChunkLocal();
                            chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 15);
                            lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                        }
                    }
                }
            } else
            {
                //Fill inner core
                for(uint x = 1; x < 15; x++)
                {
                    for(uint y = lowestEmptyHeight + 1; y < Constants.MAX_BUILD_HEIGHT; y++)
                    {
                        for(uint z = 1; z < 15; z++)
                        {
                            chunk.LightMap.SetSunLightIntensityAt(x, y, z, 15);
                        }
                    }
                }

                //Fill bottom
                for(int x = 0; x < 16; x++)
                {
                    for(int z = 0; z < 16; z++)
                    {
                        Vector3i chunkLocalPos = new Vector3i(x, (int)lowestEmptyHeight, z).ToChunkLocal();
                        chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 15);
                        lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                    }
                }

                //Fill top sides
                for(int x = 0; x < 16; x++)
                {
                    for(int z = 0; z < 16; z++)
                    {
                        if(x == 0 || x == 15 || z == 0 || z == 15)
                        {
                            if(!chunk.GetBlockAt(x, 255, z).GetBlock().IsOpaque)
                            {
                                Vector3i chunkLocalPos = new Vector3i(x, 255, z).ToChunkLocal();
                                chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 15);
                                lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                            }
                        }
                    }
                }
            }

            foreach(Chunk updatedChunk in PropagateSunlight(world, chunk, lightPropagationQueue))
            {
                if(!updatedChunks.Contains(updatedChunk))
                {
                    updatedChunks.Add(updatedChunk);
                }
            }

            return updatedChunks.ToArray();
        }

        public static Chunk[] RepairSunlightGridOnBlockAdded(World world, Chunk chunk, Vector3i blockPos, BlockState blockState)
        {
            HashSet<Chunk> updatedChunks = new HashSet<Chunk>();

            Vector3i chunkLocalPos = blockPos.ToChunkLocal();
            Queue<LightRemoveNode> darknessPropagationQueue = new Queue<LightRemoveNode>();
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();

            uint currentLightValue = chunk.LightMap.GetSunLightIntensityAt(chunkLocalPos);
            darknessPropagationQueue.Enqueue(new LightRemoveNode(chunk, chunkLocalPos, currentLightValue));
            chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 0);

            foreach(Chunk updatedChunk in PropagateDarknessSunlight(world, chunk, darknessPropagationQueue, lightPropagationQueue))
                if(!updatedChunks.Contains(updatedChunk))
                    updatedChunks.Add(updatedChunk);

            foreach(Chunk updatedChunk in PropagateSunlight(world, chunk, lightPropagationQueue))
                if(!updatedChunks.Contains(updatedChunk))
                    updatedChunks.Add(updatedChunk);

            return updatedChunks.ToArray();
        }

        private static HashSet<Chunk> PropagateDarknessSunlight(World world, Chunk chunk, Queue<LightRemoveNode> darkQueue,
                Queue<LightAddNode> lightQueue)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            //Propagate light via BFS
            while(darkQueue.Count != 0)
            {
                LightRemoveNode lightRemoveNode = darkQueue.Dequeue();

                Vector3i[] neighbourPositions = lightRemoveNode.currentChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    Chunk currentChunk = lightRemoveNode.currentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;
                      
                        currentChunk = cXNeg;
                        position.X = 15;
                    } else if(position.X > 15)
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

                    uint neighbourLight = currentChunk.LightMap.GetSunLightIntensityAt(position);

                    // Any light that is darker than our current light, we remove it.
                    if((neighbourLight != 0 && neighbourLight < lightRemoveNode.currentLight) ||
                        (lightRemoveNode.currentLight == 15 && lightRemoveNode.currentChunkLocalPos.Down() == position))
                    {
                        currentChunk.LightMap.SetSunLightIntensityAt(position, 0);
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

        private static HashSet<Chunk> PropagateSunlight(World world, Chunk chunk, Queue<LightAddNode> queue)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            //Propagate light via BFS
            while(queue.Count != 0)
            {
                LightAddNode lightAddNode = queue.Dequeue();
                Vector3i chunkLocalPos = lightAddNode.currentChunkLocalPos;

                uint currentLight = lightAddNode.currentChunk.LightMap.GetSunLightIntensityAt(chunkLocalPos);
                if(currentLight <= 1)
                    continue;

                Vector3i[] neighbourPositions = currentLight == 15 ? 
                    chunkLocalPos.GetSurroundingPositionsBesidesUp() :
                    chunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    Chunk currentChunk = lightAddNode.currentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;
                   
                        currentChunk = cXNeg;
                        position.X = 15;
                    } else if(position.X > 15)
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

                    if(currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
                        continue;

                    //If our neighbour is enough darker compared to the light in the current block, propagate this light - 1 to said block,
                    //unless we are propagating down at full intensity
                    if(chunkLocalPos.Down() == position && currentLight == 15)
                    {
                        currentChunk.LightMap.SetSunLightIntensityAt(position, currentLight);
                        queue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } else if(currentChunk.LightMap.GetSunLightIntensityAt(position) < currentLight - 1)
                    {
                        currentChunk.LightMap.SetSunLightIntensityAt(position, currentLight - 1);
                        queue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    }
                }
            }

            return processedChunks;
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
                    if(!updatedChunks.Contains(updatedChunk))
                        updatedChunks.Add(updatedChunk);

                //Very bright and large lights can engulf very weak light sources.
                //Make sure we check to expand from the sources after having removed some light from previous step.
                foreach(KeyValuePair<Vector3i, BlockState> kp in chunk.LightSourceBlocks)
                {
                    Vector3i chunkLocalPos = kp.Key.ToChunkLocal();
                    uint lightValue = LightUtils.GetChannelColor(kp.Value as ILightSource, channel);
                    LightUtils.SetLightOfChannel(chunk, chunkLocalPos, channel, lightValue);
                    lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                }

                foreach(Chunk updatedChunk in PropagateLight(world, chunk, lightPropagationQueue, channel))
                    if(!updatedChunks.Contains(updatedChunk))
                        updatedChunks.Add(updatedChunk);

                lightPropagationQueue.Clear();
                darknessPropagationQueue.Clear();
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
                return RepairLightGridBlockRemoved(world, chunk, blockPos);

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
                    if(!updatedChunks.Contains(updatedChunk))
                        updatedChunks.Add(updatedChunk);

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

                Vector3i[] neighbourPositions = lightRemoveNode.currentChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    Chunk currentChunk = lightRemoveNode.currentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    } else if(position.X > 15)
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

                uint currentLight = LightUtils.GetLightOfChannel(lightAddNode.currentChunk, lightAddNode.currentChunkLocalPos, channel);
                if(currentLight <= 1)
                    continue;

                Vector3i[] neighbourPositions = lightAddNode.currentChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    Vector3i position = neighbourPositions[i];
                    Chunk currentChunk = lightAddNode.currentChunk;

                    //Update our chunk and positions accordingly, in case we go outside of the bounds of the current chunk.
                    if(position.X < 0)
                    {
                        if(!world.loadedChunks.TryGetValue(new Vector2(currentChunk.GridX - 1, currentChunk.GridZ), out Chunk cXNeg))
                            continue;

                        currentChunk = cXNeg;
                        position.X = 15;
                    } else if(position.X > 15)
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

                    if(currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
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

