using OpenTK;
using System;

namespace Minecraft
{
    class OpaqueMeshGenerator : MeshGenerator
    {
        private const float staticTopLight = 1.15F;
        private const float staticBottomLight = 0.85F;
        private const float staticSideXLight = 1F;
        private const float staticSideZLight = 0.95F;

        public OpaqueMeshGenerator(BlockModelRegistry blockModelRegistry) : base (blockModelRegistry)
        {           
        }

        protected override ChunkBufferLayout GenerateMesh(World world, Chunk chunk)
        {
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX - 1, chunk.GridZ), out Chunk cXNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX + 1, chunk.GridZ), out Chunk cXPos);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ - 1), out Chunk cZNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.GridX, chunk.GridZ + 1), out Chunk cZPos);

            for (int sectionHeight = 0; sectionHeight < chunk.Sections.Length; sectionHeight++)
            {
                Section section = chunk.Sections[sectionHeight];
                if (section == null)
                {
                    continue;
                }

                for (int localX = 0; localX < 16; localX++)
                {
                    for (int localZ = 0; localZ < 16; localZ++)
                    {
                        for (int sectionLocalY = 0; sectionLocalY < 16; sectionLocalY++)
                        {
                            BlockState state = section.GetBlockAt(localX, sectionLocalY, localZ);
                            if (state == null)
                            {
                                continue;
                            }

                            if (!blockModelRegistry.Models.TryGetValue(state.GetBlock(), out BlockModel blockModel))
                            {
                                throw new System.Exception("Could not find model for: " + state.GetBlock().GetType());
                            }

                            Vector3i localChunkBlockPos = new Vector3i(localX, sectionLocalY + sectionHeight * 16, localZ);
                            Vector3i globalBlockPos = new Vector3i(localX + chunk.GridX * 16, sectionLocalY + sectionHeight * 16, localZ + chunk.GridZ * 16);

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state, globalBlockPos);
                            float k = Maths.ConvertRange(0, 15, 0, 1, chunk.LightMap.GetBlockLightAt(localChunkBlockPos)) + 0.1f;
                            if (blockModel.DoubleSidedFaces)
                            {
                                AddFacesToMeshDualSided(faces, localChunkBlockPos, k);
                                continue;
                            }
                            AddFacesToMeshFromFront(faces, localChunkBlockPos, k);

                            bool istnt = state.GetBlock() == Blocks.Tnt;

                            if (ShouldAddEastFaceOfBlock(cXPos, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0;
                                if(localChunkBlockPos.X + 1 > 15)
                                {
                                    if(cXPos != null)
                                        l += cXPos.LightMap.GetBlockLightAt(0, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                } else
                                {
                                    l += chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X + 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                }
                                l /= 15f;
                                l += 0.1f;
                                
                                BuildMeshForSide(Direction.Right, state, localChunkBlockPos, globalBlockPos, blockModel, staticSideZLight * l);
                            }
                            if (ShouldAddWestFaceOfBlock(cXNeg, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0;
                                if(localChunkBlockPos.X - 1 < 0)
                                {
                                    if(cXNeg != null)
                                        l += cXNeg.LightMap.GetBlockLightAt(15, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                } else
                                {
                                    l += chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X - 1, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z);
                                }
                                l /= 15f;
                                l += 0.1f;

                                BuildMeshForSide(Direction.Left, state, localChunkBlockPos, globalBlockPos, blockModel, staticSideXLight * l);
                            }
                            if (ShouldAddSouthFaceOfBlock(cZNeg, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0;
                                if(localChunkBlockPos.Z - 1 < 0)
                                {
                                    if(cZNeg != null)
                                        l += cZNeg.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 15);
                                } else
                                {
                                    l += chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z - 1);
                                }
                                l /= 15f;
                                l += 0.1f;

                                BuildMeshForSide(Direction.Back, state, localChunkBlockPos, globalBlockPos, blockModel, staticSideXLight * l);
                            }
                            if (ShouldAddNorthFaceOfBlock(cZPos, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0;
                                if(localChunkBlockPos.Z + 1 > 15)
                                {
                                    if(cZPos != null)
                                        l += cZPos.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, 0);
                                } else
                                {
                                    l += chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)localChunkBlockPos.Y, (uint)localChunkBlockPos.Z + 1);
                                }
                                l /= 15f;
                                l += 0.1f;

                                BuildMeshForSide(Direction.Front, state, localChunkBlockPos, globalBlockPos, blockModel, staticSideZLight * l);
                            }
                            if (ShouldAddTopFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0.1f + chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)Math.Min(localChunkBlockPos.Y + 1, 255), (uint)localChunkBlockPos.Z) / 15f;
                                BuildMeshForSide(Direction.Top, state, localChunkBlockPos, globalBlockPos, blockModel, staticTopLight * l);
                            }
                            if (ShouldAddBottomFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                float l = 0.1f + chunk.LightMap.GetBlockLightAt((uint)localChunkBlockPos.X, (uint)Math.Max(localChunkBlockPos.Y - 1, 0), (uint)localChunkBlockPos.Z) / 15f;
                                BuildMeshForSide(Direction.Bottom, state, localChunkBlockPos, globalBlockPos, blockModel, staticBottomLight * l);
                            }
                        }
                    }
                }
            }

            return new ChunkBufferLayout()
            {
                positions = vertexPositions.ToArray(),
                textureCoordinates = textureUVs.ToArray(),
                lights = illuminations.ToArray(),
                normals = normals.ToArray(),
                indicesCount = indicesCount
            };                   
        }

        private void BuildMeshForSide(Direction direction, BlockState state, Vector3i chunkLocalPos, Vector3i globalPos, BlockModel model, float staticLightValue)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(state, globalPos, direction);
            AddFacesToMeshFromFront(faces, chunkLocalPos, staticLightValue);
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

                if (blockModelRegistry.Models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            } else
            {
                BlockState blockWest = currentSection.GetBlockAt(localX - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            }
            return false;
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

                if (blockModelRegistry.Models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            } else
            {
                BlockState blockEast = currentSection.GetBlockAt(localX + 1, localY, localZ);
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            }
            return false;
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

                if (blockModelRegistry.Models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            } else
            {
                BlockState blockNorth = currentSection.GetBlockAt(localX, localY, localZ + 1);
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            }
            return false;
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

                if (blockModelRegistry.Models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            } else
            {
                BlockState blockSouth = currentSection.GetBlockAt(localX, localY, localZ - 1);
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            }
            return false;
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

                if (blockModelRegistry.Models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            } else
            {
                BlockState blockAbove = currentSection.GetBlockAt(localX, localY + 1, localZ);
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            }
            return false;
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

                if (blockModelRegistry.Models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            } else
            {
                BlockState blockBottom = currentSection.GetBlockAt(localX, localY - 1, localZ);
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.Models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            }
            return false;
        }
    }
}
