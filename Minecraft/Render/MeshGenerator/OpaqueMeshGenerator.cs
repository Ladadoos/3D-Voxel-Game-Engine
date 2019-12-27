using OpenTK;

namespace Minecraft
{
    class OpaqueMeshGenerator : MeshGenerator
    {
        private float topLight = 0.9F;
        private float bottomLight = 2.4F;
        private float sideXLight = 1.15F;
        private float sideZLight = 1.4F;

        public OpaqueMeshGenerator(BlockModelRegistry blockModelRegistry) : base (blockModelRegistry)
        {
            
        }

        protected override Model GenerateMesh(World world, Chunk chunk)
        {
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out Chunk cXNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out Chunk cXPos);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out Chunk cZNeg);
            world.loadedChunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out Chunk cZPos);

            for (int i = 0; i < chunk.sections.Length; i++)
            {
                Section section = chunk.sections[i];
                if (section == null)
                {
                    continue;
                }

                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        for (int y = 0; y < Constants.SECTION_HEIGHT; y++)
                        {
                            BlockState state = section.blocks[x, y, z];
                            if (state == null)
                            {
                                continue;
                            }

                            if (!blockModelRegistry.models.TryGetValue(state.GetBlock(), out BlockModel blockModel))
                            {
                                throw new System.Exception("Could not find model for: " + state.GetBlock().GetType());
                            }

                            if(blockModel is ScissorModel)
                            {
                                BlockFace[] cullFaces = blockModel.GetAlwaysVisibleFaces(state);
                                AddFacesToMeshDualSided(cullFaces, state, 1);
                                continue;
                            }

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state);
                            AddFacesToMeshFromFront(faces, state, 1);

                            if (ShouldAddEastFaceOfBlock(cXPos, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Right, state, blockModel, sideZLight);
                            }
                            if (ShouldAddWestFaceOfBlock(cXNeg, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Left, state, blockModel, sideXLight);
                            }
                            if (ShouldAddSouthFaceOfBlock(cZNeg, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Back, state, blockModel, sideXLight);
                            }
                            if (ShouldAddNorthFaceOfBlock(cZPos, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Front, state, blockModel, sideZLight);
                            }
                            if (ShouldAddTopFaceOfBlock(chunk, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Top, state, blockModel, topLight);
                            }
                            if (ShouldAddBottomFaceOfBlock(chunk, section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Bottom, state, blockModel, bottomLight);
                            }
                        }
                    }
                }
            }

            return new Model(vertexPositions.ToArray(), textureUVs.ToArray(), illumations.ToArray(), normals.ToArray(), indicesCount);        
        }

        private void BuildMeshForSide(Direction blockSide, BlockState state, BlockModel model, float lightValue)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(blockSide, state);
            AddFacesToMeshFromFront(faces, state, lightValue);
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

                BlockState blockWest = westSection.blocks[Constants.CHUNK_SIZE - 1, localY, localZ];
                if (blockWest == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockWest.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Right);
            } else
            {
                BlockState blockWest = currentSection.blocks[localX - 1, localY, localZ];
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

                BlockState blockEast = eastSection.blocks[0, localY, localZ];
                if (blockEast == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockEast.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Left);
            } else
            {
                BlockState blockEast = currentSection.blocks[localX + 1, localY, localZ];
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

                BlockState blockNorth = northSection.blocks[localX, localY, 0];
                if (blockNorth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockNorth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Back);
            } else
            {
                BlockState blockNorth = currentSection.blocks[localX, localY, localZ + 1];
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

                BlockState blockSouth = southSection.blocks[localX, localY, Constants.CHUNK_SIZE - 1];
                if (blockSouth == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockSouth.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Front);
            } else
            {
                BlockState blockSouth = currentSection.blocks[localX, localY, localZ - 1];
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

                BlockState blockAbove = sectionAbove.blocks[localX, 0, localZ];
                if (blockAbove == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockAbove.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Bottom);
            } else
            {
                BlockState blockAbove = currentSection.blocks[localX, localY + 1, localZ];
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

                BlockState blockBottom = sectionBelow.blocks[localX, Constants.SECTION_HEIGHT - 1, localZ];
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            } else
            {
                BlockState blockBottom = currentSection.blocks[localX, localY - 1, localZ];
                if (blockBottom == null)
                    return true;

                if (blockModelRegistry.models.TryGetValue(blockBottom.GetBlock(), out BlockModel blockModel))
                    return !blockModel.IsOpaqueOnSide(Direction.Top);
            }
            return false;
        }
    }
}
