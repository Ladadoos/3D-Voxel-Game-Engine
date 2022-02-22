using OpenTK;
using System;

namespace Minecraft
{
    class OpaqueMeshGenerator : MeshGenerator
    {
        private const uint staticTopLight = 60;
        private const uint staticBottomLight = 36;
        private const uint staticSideXLight = 52;
        private const uint staticSideZLight = 44;

        private SmoothLighting smoothLigher = new SmoothLighting();
        private bool smoothLighting = true;

        public OpaqueMeshGenerator(BlockModelRegistry blockModelRegistry) : base (blockModelRegistry)
        {           
        }

        protected override ChunkBufferLayout GenerateMesh(World world, Chunk chunk)
        {
            world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg);
            world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos);
            world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg);
            world.LoadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos);

            Light[] lightBuffer = new Light[4];

            for (int sectionHeight = 0; sectionHeight < chunk.Sections.Length; sectionHeight++)
            {
                Section section = chunk.Sections[sectionHeight];
                if (section == null || section.IsEmpty)
                    continue;

                for (int localX = 0; localX < 16; localX++)
                {
                    for (int localZ = 0; localZ < 16; localZ++)
                    {
                        for (int sectionLocalY = 0; sectionLocalY < 16; sectionLocalY++)
                        {
                            BlockState state = section.GetBlockAt(localX, sectionLocalY, localZ);
                            if (state == null)
                                continue;

                            BlockModel blockModel = blockModelRegistry.Models[state.GetBlock().ID];

                            Vector3i localChunkBlockPos = new Vector3i(localX, sectionLocalY + sectionHeight * 16, localZ);
                            Vector3i worldBlockPos = new Vector3i(localX + chunk.GridX * 16, sectionLocalY + sectionHeight * 16, localZ + chunk.GridZ * 16);

                            uint averageRed = 0;
                            uint averageBlue = 0;
                            uint averageGreen = 0;
                            int numAverages = 0;

                            if (ShouldAddEastFaceOfBlock(cXPos, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();
                                if(localChunkBlockPos.X + 1 > 15)
                                {
                                    if(cXPos != null)
                                        light = cXPos.LightMap.GetLightColorAt(0, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z, 4);
                                } else
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X + 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z, 4);

                                light.SetBrightness(staticSideZLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)               
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, localChunkBlockPos.X, localChunkBlockPos.Y, localChunkBlockPos.Z, Direction.Right);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Right, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }
                            if (ShouldAddWestFaceOfBlock(cXNeg, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();
                                if(localChunkBlockPos.X - 1 < 0)
                                {
                                    if(cXNeg != null)
                                        light = cXNeg.LightMap.GetLightColorAt(15, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z, 4);
                                } else
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X - 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z, 4);

                                light.SetBrightness(staticSideZLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, localChunkBlockPos.X, localChunkBlockPos.Y, localChunkBlockPos.Z, Direction.Left);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Left, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }
                            if (ShouldAddSouthFaceOfBlock(cZNeg, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();
                                if(localChunkBlockPos.Z - 1 < 0)
                                {
                                    if(cZNeg != null)
                                        light = cZNeg.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 15, 4);
                                } else
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z - 1, 4);
                                                           
                                light.SetBrightness(staticSideXLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, localChunkBlockPos.X, localChunkBlockPos.Y, localChunkBlockPos.Z, Direction.Back);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Back, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }
                            if (ShouldAddNorthFaceOfBlock(cZPos, section, localX, sectionLocalY, localZ))
                            {
                                Light light = new Light();
                                if(localChunkBlockPos.Z + 1 > 15)
                                {
                                    if(cZPos != null)
                                        light = cZPos.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 0, 4);
                                } else
                                    light = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z + 1, 4);

                                light.SetBrightness(staticSideXLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, localChunkBlockPos.X, localChunkBlockPos.Y, localChunkBlockPos.Z, Direction.Front);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Front, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }
                            if (ShouldAddTopFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                uint chunkLocalX = (uint)localChunkBlockPos.X;
                                uint worldY = (uint)Math.Min(localChunkBlockPos.Y + 1, 255);
                                uint chunkLocalZ = (uint)localChunkBlockPos.Z;

                                Light light = chunk.LightMap.GetLightColorAt(chunkLocalX, worldY, chunkLocalZ, 4);
                                light.SetBrightness(staticTopLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, (int)chunkLocalX, localChunkBlockPos.Y, (int)chunkLocalZ, Direction.Top);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Top, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }
                            if (ShouldAddBottomFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                uint chunkLocalX = (uint)localChunkBlockPos.X;
                                uint worldY = (uint)Math.Max(localChunkBlockPos.Y - 1, 0);
                                uint chunkLocalZ = (uint)localChunkBlockPos.Z;

                                Light light = chunk.LightMap.GetLightColorAt(chunkLocalX, worldY, chunkLocalZ, 4);
                                light.SetBrightness(staticBottomLight);

                                averageRed += light.GetRedChannel();
                                averageGreen += light.GetGreenChannel();
                                averageBlue += light.GetBlueChannel();
                                numAverages++;

                                if(smoothLighting)
                                    lightBuffer = smoothLigher.GetLightsAt(world, chunk, (int)chunkLocalX, localChunkBlockPos.Y, (int)chunkLocalZ, Direction.Bottom);
                                else
                                    for(int i = 0; i < 4; i++) lightBuffer[i] = light;

                                BuildMeshForSide(Direction.Bottom, state, localChunkBlockPos, worldBlockPos, blockModel, lightBuffer);
                            }

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state, worldBlockPos);
                            if(faces.Length != 0)
                            {
                                Light lightAlwaysyVisibleFaces = new Light();
                                if(!state.GetBlock().IsOpaque)
                                {
                                    lightAlwaysyVisibleFaces = chunk.LightMap.GetLightColorAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z, 4);
                                } else if(numAverages != 0)
                                {
                                    lightAlwaysyVisibleFaces.SetRedChannel((uint)(averageRed / numAverages * 4));
                                    lightAlwaysyVisibleFaces.SetGreenChannel((uint)(averageGreen / numAverages * 4));
                                    lightAlwaysyVisibleFaces.SetBlueChannel((uint)(averageBlue / numAverages * 4));
                                }
                                lightAlwaysyVisibleFaces.SetSunlight(chunk.LightMap.GetSunLightIntensityAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z) * 4);
                                lightAlwaysyVisibleFaces.SetBrightness(16 * 4 - 1);

                                for(int i = 0; i < 4; i++) lightBuffer[i] = lightAlwaysyVisibleFaces;

                                if(blockModel.DoubleSidedFaces)
                                {
                                    AddFacesToMeshDualSided(faces, localChunkBlockPos, lightBuffer, false);
                                } else
                                {
                                    AddFacesToMeshFromFront(faces, localChunkBlockPos, lightBuffer, false);
                                }
                            }
                        }
                    }
                }
            }

            return new ChunkBufferLayout()
            {
                VertexPositions = vertexPositions,
                PositionsPointer = positionPointer,
                VertexUVs = vertexUVs,
                UVsPointer = uvsPointer,
                VertexLights = vertexLights,
                LightsPointer = lightsPointer,
                VertexNormals = vertexNormals,
                NormalsPointer = normalPointer,
                IndicesCount = indicesCount
            };                   
        }

        private void BuildMeshForSide(Direction direction, BlockState state, Vector3i chunkLocalPos, Vector3i globalPos, BlockModel model, Light[] lights)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(state, globalPos, direction);
            AddFacesToMeshFromFront(faces, chunkLocalPos, lights, FlipTriangles(lights));
        }

        private bool FlipTriangles(Light[] buffer)
        {
            var (r1, g1, b1, s1, br1) = Light.Add(buffer[0], buffer[2]);
            var (r2, g2, b2, s2, br2) = Light.Add(buffer[1], buffer[3]);
            return r1 + g1 + b1 + s1 > r2 + g2 + b2 + s2;
        }

        private bool ShouldAddWestFaceOfBlock(Chunk westChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX - 1 < 0)
            {
                if (westChunk == null)
                    return true;

                Section westSection = westChunk.Sections[currentSection.Height];
                if (westSection == null)
                    return true;

                BlockState blockWest = westSection.GetBlockAt(16 - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                return !blockModelRegistry.Models[blockWest.GetBlock().ID].IsOpaqueOnSide(Direction.Right);
            } else
            {
                BlockState blockWest = currentSection.GetBlockAt(localX - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                return !blockModelRegistry.Models[blockWest.GetBlock().ID].IsOpaqueOnSide(Direction.Right);
            }
        }

        private bool ShouldAddEastFaceOfBlock(Chunk eastChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX + 1 >= 16)
            {
                if (eastChunk == null)
                    return true;

                Section eastSection = eastChunk.Sections[currentSection.Height];
                if (eastSection == null)
                    return true;

                BlockState blockEast = eastSection.GetBlockAt(0, localY, localZ);
                if (blockEast == null)
                    return true;

                return !blockModelRegistry.Models[blockEast.GetBlock().ID].IsOpaqueOnSide(Direction.Left);
            } else
            {
                BlockState blockEast = currentSection.GetBlockAt(localX + 1, localY, localZ);
                if (blockEast == null)
                    return true;

                return !blockModelRegistry.Models[blockEast.GetBlock().ID].IsOpaqueOnSide(Direction.Left);
            }
        }

        private bool ShouldAddNorthFaceOfBlock(Chunk northChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localZ + 1 >= 16)
            {
                if (northChunk == null)
                    return true;

                Section northSection = northChunk.Sections[currentSection.Height];
                if (northSection == null)
                    return true;

                BlockState blockNorth = northSection.GetBlockAt(localX, localY, 0);
                if (blockNorth == null)
                    return true;

                return !blockModelRegistry.Models[blockNorth.GetBlock().ID].IsOpaqueOnSide(Direction.Back);
            } else
            {
                BlockState blockNorth = currentSection.GetBlockAt(localX, localY, localZ + 1);
                if (blockNorth == null)
                    return true;

                return !blockModelRegistry.Models[blockNorth.GetBlock().ID].IsOpaqueOnSide(Direction.Back);
            }
        }

        private bool ShouldAddSouthFaceOfBlock(Chunk southChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localZ - 1 < 0)
            {
                if (southChunk == null)
                    return true;

                Section southSection = southChunk.Sections[currentSection.Height];
                if (southSection == null)
                    return true;

                BlockState blockSouth = southSection.GetBlockAt(localX, localY, 16 - 1);
                if (blockSouth == null)
                    return true;

                return !blockModelRegistry.Models[blockSouth.GetBlock().ID].IsOpaqueOnSide(Direction.Front);
            } else
            {
                BlockState blockSouth = currentSection.GetBlockAt(localX, localY, localZ - 1);
                if (blockSouth == null)
                    return true;

                return !blockModelRegistry.Models[blockSouth.GetBlock().ID].IsOpaqueOnSide(Direction.Front);
            }
        }

        private bool ShouldAddTopFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY + 1 >= 16)
            {
                if (currentSection.Height == 16 - 1)
                    return true;

                Section sectionAbove = currentChunk.Sections[currentSection.Height + 1];
                if (sectionAbove == null)
                    return true;

                BlockState blockAbove = sectionAbove.GetBlockAt(localX, 0, localZ);
                if (blockAbove == null)
                    return true;

                return !blockModelRegistry.Models[blockAbove.GetBlock().ID].IsOpaqueOnSide(Direction.Bottom);
            } else
            {
                BlockState blockAbove = currentSection.GetBlockAt(localX, localY + 1, localZ);
                if (blockAbove == null)
                    return true;

                return !blockModelRegistry.Models[blockAbove.GetBlock().ID].IsOpaqueOnSide(Direction.Bottom);
            }
        }

        private bool ShouldAddBottomFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY - 1 < 0)
            {
                if (currentSection.Height == 0)
                    return true;

                Section sectionBelow = currentChunk.Sections[currentSection.Height - 1];
                if (sectionBelow == null)
                    return true;

                BlockState blockBottom = sectionBelow.GetBlockAt(localX, 16 - 1, localZ);
                if (blockBottom == null)
                    return true;

                return !blockModelRegistry.Models[blockBottom.GetBlock().ID].IsOpaqueOnSide(Direction.Top);
            } else
            {
                BlockState blockBottom = currentSection.GetBlockAt(localX, localY - 1, localZ);
                if (blockBottom == null)
                    return true;

                return !blockModelRegistry.Models[blockBottom.GetBlock().ID].IsOpaqueOnSide(Direction.Top);
            }
        }
    }
}
