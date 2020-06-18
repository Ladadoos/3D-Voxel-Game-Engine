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
            public readonly Chunk Chunk;
            public readonly Vector3i ChunkLocalPos;

            public LightAddNode(Chunk currentChunk, Vector3i currentChunkLocalPos)
            {
                Chunk = currentChunk;
                ChunkLocalPos = currentChunkLocalPos;
            }
        }

        /// <summary>
        /// Struct used in the BFS algorithm used to remove light 
        /// </summary>
        struct LightRemoveNode
        {
            public readonly Chunk Chunk;
            public readonly Vector3i ChunkLocalPos;
            public readonly uint LightValue;

            public LightRemoveNode(Chunk currentChunk, Vector3i currentChunkLocalPos, uint currentLight)
            {
                Chunk = currentChunk;
                ChunkLocalPos = currentChunkLocalPos;
                LightValue = currentLight;
            }
        }

        private static BlockPropagation blockPropagation = new BlockPropagation();

        public static Chunk[] RepairSunlightGridBlockRemoved(World world, Chunk chunk, Vector3i blockPos)
        {
            return RepairSunlightGridOnBlockAdded(world, chunk, blockPos, null);
        }

        public static Chunk[] GenerateInitialSunlightGrid(World world, Chunk chunk)
        {
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();

            uint lowestEmptySection = chunk.GetLowestEmptySectionAfterEachOtherFromTop();
            uint lowestEmptyHeight = lowestEmptySection * 16;
            if(lowestEmptySection == 15)
            {
                //If the lowest empty section is the top most one, then start light propagation from the top
                for(int x = 0; x < 16; x++)
                {
                    for(int z = 0; z < 16; z++)
                    {
                        if(!chunk.GetBlockAt(x, Constants.MAX_BUILD_HEIGHT - 1, z).GetBlock().IsOpaque)
                        {
                            Vector3i chunkLocalPos = new Vector3i(x, Constants.MAX_BUILD_HEIGHT - 1, z).ToChunkLocal();
                            chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 15);
                            lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                        }
                    }
                }
            } else
            {
                //If the lowest empty section is lower than the top most possible section, then that means all
                //of the blocks in all of the sections above this section are exposed to be sun and can more
                //quickly be filled with sun light. The edges are still checked for light propagation

                //Fill inner core
                for(uint x = 1; x < 15; x++)
                    for(uint y = lowestEmptyHeight + 1; y < Constants.MAX_BUILD_HEIGHT; y++)
                        for(uint z = 1; z < 15; z++)
                            chunk.LightMap.SetSunLightIntensityAt(x, y, z, 15);

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
                            if(!chunk.GetBlockAt(x, Constants.MAX_BUILD_HEIGHT - 1, z).GetBlock().IsOpaque)
                            {
                                Vector3i chunkLocalPos = new Vector3i(x, Constants.MAX_BUILD_HEIGHT - 1, z).ToChunkLocal();
                                chunk.LightMap.SetSunLightIntensityAt(chunkLocalPos, 15);
                                lightPropagationQueue.Enqueue(new LightAddNode(chunk, chunkLocalPos));
                            }
                        }
                    }
                }
            }

            HashSet<Chunk> updatedChunks = new HashSet<Chunk>();
            foreach(Chunk updatedChunk in PropagateSunlight(world, chunk, lightPropagationQueue))
                if(!updatedChunks.Contains(updatedChunk))
                    updatedChunks.Add(updatedChunk);

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

            blockPropagation.Begin();

            //Propagate light via BFS
            while(darkQueue.Count != 0)
            {
                LightRemoveNode lightRemoveNode = darkQueue.Dequeue();

                Vector3i[] neighbourPositions = lightRemoveNode.ChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    (Vector3i position, Chunk currentChunk) = blockPropagation.FixReference(world, neighbourPositions[i],
                        lightRemoveNode.Chunk, out bool referenceFixable);

                    if(!referenceFixable || currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
                        continue;

                    uint neighbourLight = currentChunk.LightMap.GetSunLightIntensityAt(position);

                    // Any light that is darker than our current light, we remove it.
                    if((neighbourLight != 0 && neighbourLight < lightRemoveNode.LightValue) ||
                        (lightRemoveNode.LightValue == 15 && lightRemoveNode.ChunkLocalPos.Down() == position))
                    {
                        currentChunk.LightMap.SetSunLightIntensityAt(position, 0);
                        darkQueue.Enqueue(new LightRemoveNode(currentChunk, position, neighbourLight));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } //Make sure to propagate already existing light from other sources to the areas that are now dark.
                    else if(neighbourLight >= lightRemoveNode.LightValue)
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

            blockPropagation.Begin();

            //Propagate light via BFS
            while(queue.Count != 0)
            {
                LightAddNode lightAddNode = queue.Dequeue();
                Vector3i chunkLocalPos = lightAddNode.ChunkLocalPos;

                uint currentLight = lightAddNode.Chunk.LightMap.GetSunLightIntensityAt(chunkLocalPos);
                if(currentLight <= 1)
                    continue;

                Vector3i[] neighbourPositions = currentLight == 15 ? 
                    chunkLocalPos.GetSurroundingPositionsBesidesUp() :
                    chunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    (Vector3i position, Chunk currentChunk) = blockPropagation.FixReference(world, neighbourPositions[i],
                        lightAddNode.Chunk, out bool referenceFixable);

                    if(!referenceFixable || currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
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

                foreach(Chunk updatedChunk in PropagateBlockDarness(world, chunk, darknessPropagationQueue, lightPropagationQueue, channel))
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

                foreach(Chunk updatedChunk in PropagateBlockLight(world, chunk, lightPropagationQueue, channel))
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
            Queue<LightRemoveNode> darknessPropagationQueue = new Queue<LightRemoveNode>();
            Queue<LightAddNode> lightPropagationQueue = new Queue<LightAddNode>();

            foreach(LightChannel channel in LightUtils.BlockVisibileColorChannels)
            {
                uint currentLightValue = LightUtils.GetLightOfChannel(chunk, sourceChunkLocalPos, channel);
                darknessPropagationQueue.Enqueue(new LightRemoveNode(chunk, sourceChunkLocalPos, currentLightValue));
                LightUtils.SetLightOfChannel(chunk, sourceChunkLocalPos, channel, 0);

                foreach(Chunk updatedChunk in PropagateBlockDarness(world, chunk, darknessPropagationQueue, lightPropagationQueue, channel))
                    if(!updatedChunks.Contains(updatedChunk))
                        updatedChunks.Add(updatedChunk);

                uint lightValue = LightUtils.GetChannelColor(lightSource, channel);
                LightUtils.SetLightOfChannel(chunk, sourceChunkLocalPos, channel, lightValue);
                lightPropagationQueue.Enqueue(new LightAddNode(chunk, sourceChunkLocalPos));

                foreach(Chunk updatedChunk in PropagateBlockLight(world, chunk, lightPropagationQueue, channel))
                    if(!updatedChunks.Contains(updatedChunk))
                        updatedChunks.Add(updatedChunk);

                darknessPropagationQueue.Clear();
                lightPropagationQueue.Clear();
            }

            return updatedChunks.ToArray();
        }

        private static HashSet<Chunk> PropagateBlockDarness(World world, Chunk chunk, Queue<LightRemoveNode> darkQueue,
            Queue<LightAddNode> lightQueue, LightChannel channel)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            blockPropagation.Begin();

            //Propagate light via BFS
            while(darkQueue.Count != 0)
            {
                LightRemoveNode lightRemoveNode = darkQueue.Dequeue();

                Vector3i[] neighbourPositions = lightRemoveNode.ChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    (Vector3i position, Chunk currentChunk) = blockPropagation.FixReference(world, neighbourPositions[i],
                        lightRemoveNode.Chunk, out bool referenceFixable);

                    if(!referenceFixable || currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
                        continue;

                    uint neighbourLight = LightUtils.GetLightOfChannel(currentChunk, position, channel);

                    // Any light that is darker than our current light, we remove it
                    if(neighbourLight > 0 && neighbourLight < lightRemoveNode.LightValue)
                    {
                        LightUtils.SetLightOfChannel(currentChunk, position, channel, 0);
                        darkQueue.Enqueue(new LightRemoveNode(currentChunk, position, neighbourLight));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } //Make sure to propagate already existing light from other sources to the areas that are now dark
                    else if(neighbourLight >= lightRemoveNode.LightValue)
                    {
                        lightQueue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    } 
                }
            }

            return processedChunks;
        }

        private static HashSet<Chunk> PropagateBlockLight(World world, Chunk chunk, Queue<LightAddNode> lightQueue, LightChannel channel)
        {
            HashSet<Chunk> processedChunks = new HashSet<Chunk>();

            blockPropagation.Begin();

            //Propagate light via BFS
            while(lightQueue.Count != 0)
            {
                LightAddNode lightAddNode = lightQueue.Dequeue();

                uint currentLight = LightUtils.GetLightOfChannel(lightAddNode.Chunk, lightAddNode.ChunkLocalPos, channel);
                if(currentLight <= 1)
                    continue;

                Vector3i[] neighbourPositions = lightAddNode.ChunkLocalPos.GetSurroundingPositions();
                for(int i = 0; i < neighbourPositions.Length; i++)
                {
                    (Vector3i position, Chunk currentChunk) = blockPropagation.FixReference(world, neighbourPositions[i],
                        lightAddNode.Chunk, out bool referenceFixable);

                    if(!referenceFixable || currentChunk.GetBlockAt(position).GetBlock().IsOpaque)
                        continue;

                    //If our neighbour is enough darker compared to the light in the current block, propagate this light - 1 to said block.
                    uint neighbourLight = LightUtils.GetLightOfChannel(currentChunk, position, channel);
                    if(neighbourLight < currentLight - 1)
                    {
                        LightUtils.SetLightOfChannel(currentChunk, position, channel, currentLight - 1);
                        lightQueue.Enqueue(new LightAddNode(currentChunk, position));

                        if(currentChunk != chunk && !processedChunks.Contains(currentChunk))
                            processedChunks.Add(currentChunk);
                    }
                }
            }

            return processedChunks;
        }
    }
}

