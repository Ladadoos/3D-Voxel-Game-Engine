using System;
using System.Collections.Generic;

namespace Minecraft
{
    class FloodFillLight
    {
        public void GenerateLightGrid(World world, Chunk chunk)
        {
           // world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg);
           // world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos);
           // world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg);
           // world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos);

            chunk.LightMap.Reset();

            Queue<Vector3i> queue = new Queue<Vector3i>();
            foreach(KeyValuePair<Vector3i, BlockState> kp in chunk.LightSourceBlocks)
            {
                Vector3i localPos = kp.Key.ToChunkLocal();
                chunk.LightMap.SetBlockLightAt(localPos, kp.Value.GetBlock().LightIntensity);
                queue.Enqueue(localPos);
            }

            while(queue.Count != 0)
            {     
                Vector3i pos = queue.Dequeue();

                if(pos.X < 0 || pos.X > 15 || pos.Z < 0 || pos.Z > 15 || pos.Y < 0 || pos.Y > 255)
                {
                    continue;
                }

                uint lightValue = chunk.LightMap.GetBlockLightAt(pos);
                if(lightValue - 1 <= 0)
                {
                    continue;
                }
                
                foreach(Vector3i position in pos.GetSurroundingPositions())
                {
                    if(position.X < 0 || position.X > 15 || position.Z < 0 || position.Z > 15 || position.Y < 0 || position.Y > 255)
                    {
                        continue;
                    }

                    uint neighLightValue = chunk.LightMap.GetBlockLightAt(position);

                    Vector3i worldPos = new Vector3i(position.X + chunk.GridX * 16, position.Y, position.Z + chunk.GridZ * 16);
                    bool canGoThrough = !world.GetBlockAt(worldPos).GetBlock().IsOpaque;
                    bool isLower = neighLightValue < lightValue - 1;

                    if(isLower)
                    {
                        if(lightValue - 1 <= 0)
                            throw new Exception();

                        chunk.LightMap.SetBlockLightAt(position, lightValue - 1);
                        if(canGoThrough)
                        {
                            queue.Enqueue(position);
                        } 
                    }
                }
            }
        }
    }
}

