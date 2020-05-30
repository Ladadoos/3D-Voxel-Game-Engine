using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Minecraft
{
    struct GenerateChunkRequest
    {
        public int playerId; //The id of the player who requested for this chunk to be generated
        public Vector2 gridPosition; //At what chunk grid position the chunk should be generated
        public World world; //In what would the chunk should be generated
        public Action<GenerateChunkOutput> callback;
    }

    struct GenerateChunkOutput
    {
        public Chunk chunk; //The generated chunk
        public World world; //The world in which the chunk was generated
    }

    class WorldGenerator
    {
        private readonly MountainBiome mountainBiome = new MountainBiome();
        private readonly ForestBiome forestBiome = new ForestBiome();
        private readonly DesertBiome desertBiome = new DesertBiome();

        private const double temperatureDetail = 0.0075D;
        private readonly Noise2DPerlin temperatureFunction = new Noise2DPerlin();

        private const double moistureDetail = 0.0075D;
        private readonly Noise2DPerlin moistureFunction = new Noise2DPerlin(25555);

        private readonly BiomeProvider biomeProvider;

        private readonly Biome[] registeredBiomes;
        private const int activeBiomes = 3;
        public readonly int SeaLevel = 100;

        private readonly object generationLock = new object();
        private readonly Dictionary<Tuple<World, Vector2>, List<GenerateChunkRequest>> chunkGenerationRequests = new Dictionary<Tuple<World, Vector2>, List<GenerateChunkRequest>>();
        private readonly Queue<GenerateChunkRequest> chunkGenerationOrder = new Queue<GenerateChunkRequest>();
        private readonly Thread terrainGeneratorThread;

        public WorldGenerator()
        {
            registeredBiomes = new Biome[activeBiomes]
            {
                mountainBiome,
                desertBiome,
                forestBiome,
            };

            biomeProvider = new BiomeProvider(registeredBiomes);

            terrainGeneratorThread = new Thread(ChunkGeneratorThread);
            terrainGeneratorThread.IsBackground = true;
            terrainGeneratorThread.Start();
        }

        public void AddChunkGenerationRequest(GenerateChunkRequest request)
        {
            var tuple = new Tuple<World, Vector2>(request.world, request.gridPosition);

            lock(generationLock)
            {
                //Check if anyone else already requested for a chunk to be generated at this position
                if(chunkGenerationRequests.TryGetValue(tuple, out List<GenerateChunkRequest> requests))
                {
                    requests.Add(request);
                } else //Else just start a new list of the request
                {
                    chunkGenerationRequests.Add(tuple, new List<GenerateChunkRequest>() { request });
                }

                //Chunk generation requests are handled in a first-come-first-serve manner
                chunkGenerationOrder.Enqueue(request);
            }
        }

        private void ChunkGeneratorThread()
        {  
            List<GenerateChunkRequest> allRequests = new List<GenerateChunkRequest>();
            GenerateChunkRequest request = new GenerateChunkRequest();
            Chunk chunk = null;

            while(true)
            {
                Thread.Sleep(5);

                bool shouldGenerateChunk = false;             
                lock(generationLock)
                {
                    if(chunkGenerationOrder.Count > 0)
                    {
                        //Dequeue the oldest chunk generation request
                        request = chunkGenerationOrder.Dequeue();

                        var tuple = new Tuple<World, Vector2>(request.world, request.gridPosition);
                        if(chunkGenerationRequests.TryGetValue(tuple, out List<GenerateChunkRequest> requests))
                        {
                            shouldGenerateChunk = true;
                            chunkGenerationRequests.Remove(tuple);
                            allRequests = new List<GenerateChunkRequest>(requests);
                        } else
                        {
                            throw new ArgumentException();
                        }
                    }
                }

                if(shouldGenerateChunk)
                {
                    chunk = GenerateBlocksForChunkAt((int)request.gridPosition.X, (int)request.gridPosition.Y);

                    GenerateChunkOutput answer = new GenerateChunkOutput()
                    {
                        chunk = chunk,
                        world = request.world
                    };

                    //Multiple people might have requested for this chunk to also be generated. We reply to those too
                    //while we are at it
                    foreach(var outputEntry in allRequests)
                    {
                        outputEntry.callback.Invoke(answer);
                    }
                }
            }
        }

        public Chunk GenerateBlocksForChunkAt(int chunkX, int chunkY)
        {
            Chunk chunk = new Chunk(chunkX, chunkY);

            const float chunkDim = 16;

            double temperatureXOffset = 0;
            double temperatureYOffset = 0;
            temperatureYOffset = chunkX * chunkDim * temperatureDetail;

            double moistureXOffset = 0;
            double moistureYOffset = 0;
            moistureYOffset = chunkX * chunkDim * moistureDetail;

            for (int localX = 0; localX < chunkDim; localX++)
            {
                temperatureXOffset = chunkY * chunkDim * temperatureDetail;
                moistureXOffset = chunkY * chunkDim * moistureDetail;

                for (int localZ = 0; localZ < chunkDim; localZ++)
                {
                    double temperature = temperatureFunction.GetValuePositive(temperatureXOffset, temperatureYOffset);
                    double moisture = moistureFunction.GetValuePositive(moistureXOffset, moistureYOffset);

                    BiomeMembership[] biomes = biomeProvider.GetBiomeMemberships(temperature, moisture);
                    double biomeHeightAddon = 0;

                    BiomeMembership bestBiome = biomes[0];
                    foreach (BiomeMembership wBiome in biomes)
                    {
                        if (bestBiome.Percentage < wBiome.Percentage)
                        {
                            bestBiome = wBiome;
                        }
                        biomeHeightAddon += wBiome.Percentage * wBiome.Biome.OffsetAt(chunkX, chunkY, localX, localZ);
                    }
                    int worldY = SeaLevel + (int)biomeHeightAddon;

                    chunk.AddBlockAt(localX, worldY, localZ, bestBiome.Biome.TopBlock.GetNewDefaultState());
                    for (int k = worldY - 1; k >= worldY - 3; k--)
                    {
                        chunk.AddBlockAt(localX, k, localZ, bestBiome.Biome.GradiantBlock.GetNewDefaultState());
                    }

                    worldY -= 3;
                    for (int k = worldY - 1; k >= 0; k--)
                    {
                        chunk.AddBlockAt(localX, k, localZ, Blocks.Stone.GetNewDefaultState());
                    }

                    worldY += 4;
                    bestBiome.Biome.Decorator.Decorate(chunk, worldY, localX, localZ);

                    temperatureXOffset += temperatureDetail;
                    moistureXOffset += moistureDetail;
                }

                temperatureYOffset += temperatureDetail;
                moistureYOffset += moistureDetail;
            }

            return chunk;
        }
    }
}
