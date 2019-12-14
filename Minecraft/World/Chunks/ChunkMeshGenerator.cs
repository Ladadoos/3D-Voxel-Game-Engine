using System.Collections.Generic;
using OpenTK;

namespace Minecraft
{
    class ChunkMeshGenerator
    {
        private float topLight = 0.9F;
        private float bottomLight = 2.4F;
        private float sideXLight = 1.15F;
        private float sideZLight = 1.4F;

        private List<float> positions;
        private List<float> textureCoords;
        private List<float> lights;
        private int indCount;

        public ChunkMeshGenerator()
        {
            ResetData();
        }

        public void ResetData()
        {
            positions = new List<float>();
            textureCoords = new List<float>();
            lights = new List<float>();
            indCount = 0;

            activeCurrentChunk = null;
        }

        private Chunk activeCurrentChunk;

        public void GenerateRenderMeshForChunk(World world, MasterRenderer masterRenderer, Chunk chunk)
        {
            activeCurrentChunk = chunk;

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
                            if (state == null || state.block == Block.Air)
                            {
                                continue;
                            }
                            masterRenderer.blockModelRegistry.models.TryGetValue(state.block, out BlockModel blockModel);

                            if (blockModel == null)
                            {
                                throw new System.Exception("Could not find model for: " + state.block.GetType());
                            }

                           // BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state);
                           // AddBlockFacesToMesh(faces, state, 1);

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
                            if (ShouldAddTopFaceOfBlock(section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Top, state, blockModel, topLight);
                            }
                            if (ShouldAddBottomFaceOfBlock(section, x, y, z))
                            {
                                BuildMeshForSide(Direction.Bottom, state, blockModel, bottomLight);
                            }
                        }
                    }
                }
            }

            Model hardBlocksChunkModel = new Model(positions.ToArray(), textureCoords.ToArray(), lights.ToArray(), indCount);
            RenderChunk renderChunk = new RenderChunk(hardBlocksChunkModel, chunk.gridX, chunk.gridZ);
            masterRenderer.AddChunkToRender(renderChunk);

            ResetData();
        }

        private void BuildMeshForSide(Direction blockSide, BlockState state, BlockModel model, float lightValue)
        {
            BlockFace[] faces = model.GetPartialVisibleFaces(blockSide, state);
            AddBlockFacesToMesh(faces, state, lightValue);
        }

        private void AddBlockFacesToMesh(BlockFace[] faces, BlockState state, float lightValue)
        {
            if (faces.Length > 0)
            {
                foreach (BlockFace face in faces)
                {
                    foreach (float uv in face.textureCoords)
                    {
                        textureCoords.Add(uv);
                    }

                    foreach (Vector3 position in face.positions)
                    {
                        Vector3 world = position + new Vector3(state.ChunkLocalPosition());
                        positions.Add(world.X);
                        positions.Add(world.Y);
                        positions.Add(world.Z);
                    }

                    indCount += 4;
                    lights.Add(lightValue);
                    lights.Add(lightValue);
                    lights.Add(lightValue);
                    lights.Add(lightValue);
                }
            }
        }

        private bool ShouldAddWestFaceOfBlock(Chunk westChunk, Section currentSection, float x, float y, float z)
        {
            if (x - 1 < 0)
            {
                Section westSection = null;
                if (westChunk != null)
                {
                    westSection = westChunk.sections[currentSection.height];
                }

                if (westSection != null)
                {
                    if (westSection.blocks[Constants.CHUNK_SIZE - 1, (int)y, (int)z] == null)
                    {
                        return true;
                    }
                } else
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x - 1, (int)y, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddEastFaceOfBlock(Chunk eastChunk, Section currentSection, float x, float y, float z)
        {
            if (x + 1 >= Constants.CHUNK_SIZE)
            {
                Section eastSection = null;
                if (eastChunk != null)
                {
                    eastSection = eastChunk.sections[currentSection.height];
                }

                if (eastSection != null)
                {
                    if (eastSection.blocks[0, (int)y, (int)z] == null)
                    {
                        return true;
                    }
                } else
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x + 1, (int)y, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddNorthFaceOfBlock(Chunk northChunk, Section currentSection, float x, float y, float z)
        {
            if (z + 1 >= Constants.CHUNK_SIZE)
            {
                Section northSection = null;
                if (northChunk != null)
                {
                    northSection = northChunk.sections[currentSection.height];
                }

                if (northSection != null)
                {
                    if (northSection.blocks[(int)x, (int)y, 0] == null)
                    {
                        return true;
                    }
                } else
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x, (int)y, (int)z + 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddSouthFaceOfBlock(Chunk southChunk, Section currentSection, float x, float y, float z)
        {
            if (z - 1 < 0)
            {
                Section southSection = null;
                if (southChunk != null)
                {
                    southSection = southChunk.sections[currentSection.height];
                }

                if (southSection != null)
                {
                    if (southSection.blocks[(int)x, (int)y, Constants.CHUNK_SIZE - 1] == null)
                    {
                        return true;
                    }
                } else
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x, (int)y, (int)z - 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddTopFaceOfBlock(Section currentSection, float x, float y, float z)
        {
            if (y + 1 >= Constants.SECTION_HEIGHT)
            {
                if (currentSection.height == Constants.SECTION_HEIGHT - 1)
                {
                    return true;
                }

                Section sectionAbove = activeCurrentChunk.sections[currentSection.height + 1];
                if ((sectionAbove != null && sectionAbove.blocks[(int)x, 0, (int)z] == null) || sectionAbove == null)
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x, (int)y + 1, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddBottomFaceOfBlock(Section currentSection, float x, float y, float z)
        {
            if (y - 1 < 0)
            {
                if (currentSection.height == 0)
                {
                    return true;
                }

                Section sectionBelow = activeCurrentChunk.sections[currentSection.height - 1];
                if ((sectionBelow != null && sectionBelow.blocks[(int)x, Constants.SECTION_HEIGHT - 1, (int)z] == null) || sectionBelow == null)
                {
                    return true;
                }
            } else if (currentSection.blocks[(int)x, (int)y - 1, (int)z] == null)
            {
                return true;
            }
            return false;
        }
    }
}
