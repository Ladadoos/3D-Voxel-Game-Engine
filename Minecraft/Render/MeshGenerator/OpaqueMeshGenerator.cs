using OpenTK;

namespace Minecraft
{
    class OpaqueMeshGenerator : MeshGenerator
    {
        private float topLight = 1.35F;
        private float bottomLight = 0.75F;
        private float sideXLight = 1F;
        private float sideZLight = 1.15F;

        public OpaqueMeshGenerator(BlockModelRegistry blockModelRegistry) : base (blockModelRegistry)
        {
            
        }

        protected override ChunkBufferLayout GenerateMesh(World world, Chunk chunk)
        {
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos);

            for (int sectionHeight = 0; sectionHeight < chunk.sections.Length; sectionHeight++)
            {
                Section section = chunk.sections[sectionHeight];
                if (section == null)
                {
                    continue;
                }

                for (int localX = 0; localX < Constants.CHUNK_SIZE; localX++)
                {
                    for (int localZ = 0; localZ < Constants.CHUNK_SIZE; localZ++)
                    {
                        for (int sectionLocalY = 0; sectionLocalY < Constants.SECTION_HEIGHT; sectionLocalY++)
                        {
                            BlockState state = section.GetBlockAt(localX, sectionLocalY, localZ);
                            if (state == null)
                            {
                                continue;
                            }

                            if (!blockModelRegistry.models.TryGetValue(state.GetBlock(), out BlockModel blockModel))
                            {
                                throw new System.Exception("Could not find model for: " + state.GetBlock().GetType());
                            }

                            Vector3 blockPos = new Vector3(localX, sectionLocalY + sectionHeight * 16, localZ);
                            if (blockModel is ScissorModel)
                            {
                                BlockFace[] cullFaces = blockModel.GetAlwaysVisibleFaces(state);
                                AddFacesToMeshDualSided(cullFaces, blockPos, 1);
                                continue;
                            }

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state);
                            AddFacesToMeshFromFront(faces, blockPos, 1);

                           //System.Console.WriteLine(section.height + "-");

                            if (ShouldAddEastFaceOfBlock(cXPos, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Right, state, blockPos, blockModel, sideZLight);
                            }
                            if (ShouldAddWestFaceOfBlock(cXNeg, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Left, state, blockPos, blockModel, sideXLight);
                            }
                            if (ShouldAddSouthFaceOfBlock(cZNeg, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Back, state, blockPos, blockModel, sideXLight);
                            }
                            if (ShouldAddNorthFaceOfBlock(cZPos, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Front, state, blockPos, blockModel, sideZLight);
                            }
                            if (ShouldAddTopFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Top, state, blockPos, blockModel, topLight);
                            }
                            if (ShouldAddBottomFaceOfBlock(chunk, section, localX, sectionLocalY, localZ))
                            {
                                BuildMeshForSide(Direction.Bottom, state, blockPos, blockModel, bottomLight);
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

        private void BuildMeshForSide(Direction blockSide, BlockState state, Vector3 blockPos, BlockModel model, float lightValue)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(blockSide, state);
            AddFacesToMeshFromFront(faces, blockPos, lightValue);
        }

        private bool ShouldAddWestFaceOfBlock(Chunk westChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX - 1 < 0)
            {
                if (westChunk == null)
                    return true;

                Section westSection = westSection = westChunk.sections[currentSection.height];
                if (westSection == null)
                    return true;

                BlockState blockWest = westSection.GetBlockAt(16 - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            } else
            {
                BlockState blockWest = currentSection.GetBlockAt(localX - 1, localY, localZ);
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            }
            return false;
        }

        private bool ShouldAddEastFaceOfBlock(Chunk eastChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localX + 1 >= Constants.CHUNK_SIZE)
            {
                if (eastChunk == null)
                    return true;

                Section eastSection = eastSection = eastChunk.sections[currentSection.height];
                if (eastSection == null)
                    return true;

                BlockState blockEast = eastSection.GetBlockAt(0, localY, localZ);
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            } else
            {
                BlockState blockEast = currentSection.GetBlockAt(localX + 1, localY, localZ);
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            }
            return false;
        }

        private bool ShouldAddNorthFaceOfBlock(Chunk northChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localZ + 1 >= Constants.CHUNK_SIZE)
            {
                if (northChunk == null)
                    return true;

                Section northSection = northSection = northChunk.sections[currentSection.height];
                if (northSection == null)
                    return true;

                BlockState blockNorth = northSection.GetBlockAt(localX, localY, 0);
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            } else
            {
                BlockState blockNorth = currentSection.GetBlockAt(localX, localY, localZ + 1);
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
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

                Section southSection = southSection = southChunk.sections[currentSection.height];
                if (southSection == null)
                    return true;

                BlockState blockSouth = southSection.GetBlockAt(localX, localY, 16 - 1);
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            } else
            {
                BlockState blockSouth = currentSection.GetBlockAt(localX, localY, localZ - 1);
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            }
            return false;
        }

        private bool ShouldAddTopFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY + 1 >= Constants.SECTION_HEIGHT)
            {
                if (currentSection.height == Constants.SECTION_HEIGHT - 1)
                    return true;

                Section sectionAbove = currentChunk.sections[currentSection.height + 1];
                if (sectionAbove == null)
                    return true;

                BlockState blockAbove = sectionAbove.GetBlockAt(localX, 0, localZ);
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            } else
            {
                BlockState blockAbove = currentSection.GetBlockAt(localX, localY + 1, localZ);
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            }
            return false;
        }

        private bool ShouldAddBottomFaceOfBlock(Chunk currentChunk, Section currentSection, int localX, int localY, int localZ)
        {
            if (localY - 1 < 0)
            {
                if (currentSection.height == 0)
                    return true;

                Section sectionBelow = currentChunk.sections[currentSection.height - 1];
                if (sectionBelow == null)
                    return true;

                BlockState blockBottom = sectionBelow.GetBlockAt(localX, 16 - 1, localZ);
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            } else
            {
                BlockState blockBottom = currentSection.GetBlockAt(localX, localY - 1, localZ);
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            }
            return false;
        }
    }
}
