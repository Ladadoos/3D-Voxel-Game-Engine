using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Minecraft
{
    struct GenerateChunkRequest
    {
        public Vector2 gridPosition;
        public World world;
        public Action<GenerateChunkOutput> callback;
    }

    struct GenerateChunkOutput
    {
        public Chunk chunk;
        public World world;
    }

    struct GenerateChunkRequestOutgoing
    {
        public Vector2 gridPosition;
        public World world;
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
            lock(generationLock)
            {
                var tuple = new Tuple<World, Vector2>(request.world, request.gridPosition);
                if(chunkGenerationRequests.TryGetValue(tuple, out List<GenerateChunkRequest> requests))
                {
                    requests.Add(request);
                } else
                {
                    chunkGenerationRequests.Add(tuple, new List<GenerateChunkRequest>() { request });
                }
                chunkGenerationOrder.Enqueue(request);
            }
        }

        private void ChunkGeneratorThread()
        {
            List<Tuple<GenerateChunkOutput, Action<GenerateChunkOutput>>> generationOutput = new List<Tuple<GenerateChunkOutput, Action<GenerateChunkOutput>>>();

            while(true)
            {
                Thread.Sleep(5);

                bool generated = false;             
                lock(generationLock)
                {
                    if(chunkGenerationOrder.Count > 0)
                    {
                        GenerateChunkRequest request = chunkGenerationOrder.Dequeue();

                        var tuple = new Tuple<World, Vector2>(request.world, request.gridPosition);
                        if(chunkGenerationRequests.TryGetValue(tuple, out List<GenerateChunkRequest> allRequests))
                        {
                            Chunk chunk = GenerateBlocksForChunkAt((int)request.gridPosition.X, (int)request.gridPosition.Y);

                            foreach(var awaitingRequest in allRequests)
                            {
                                GenerateChunkOutput answer = new GenerateChunkOutput()
                                {
                                    chunk = chunk,
                                    world = request.world
                                };
                                generationOutput.Add(new Tuple<GenerateChunkOutput, Action<GenerateChunkOutput>>(answer, awaitingRequest.callback));
                            }
                            chunkGenerationRequests.Remove(tuple);
                            generated = true;
                        }
                    }
                }

                if(generated)
                {
                    foreach(var outputEntry in generationOutput)
                    {
                        outputEntry.Item2.Invoke(outputEntry.Item1);
                    }
                    generationOutput.Clear();
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
