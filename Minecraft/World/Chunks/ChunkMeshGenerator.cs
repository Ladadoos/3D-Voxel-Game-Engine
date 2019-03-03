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
        private List<int> indices;
        private List<float> lights;
        private int indCount;

        private BlockDatabase blockDatabase;
        private World world;

        public ChunkMeshGenerator(BlockDatabase blockDatabase, World world)
        {
            this.world = world;
            this.blockDatabase = blockDatabase;
            ResetData();
        }

        public void ResetData()
        {
            positions = new List<float>();
            textureCoords = new List<float>();
            indices = new List<int>();
            lights = new List<float>();
            indCount = 0;

            activeCurrentChunk = null;
        }

        private Chunk activeCurrentChunk;

        public void PrepareChunkToRender(Chunk toPrepareChunk, bool updateSurroundingChunks)
        {
            GenerateRenderMeshForChunk(toPrepareChunk);

            if (!updateSurroundingChunks)
            {
                return;
            }

            Chunk cXNeg = null;
            world.chunks.TryGetValue(new Vector2(toPrepareChunk.gridX - 1, toPrepareChunk.gridZ), out cXNeg);
            if (cXNeg != null)
            {
                GenerateRenderMeshForChunk(cXNeg);
            }

            Chunk cXPos = null;
            world.chunks.TryGetValue(new Vector2(toPrepareChunk.gridX + 1, toPrepareChunk.gridZ), out cXPos);
            if (cXPos != null)
            {
                GenerateRenderMeshForChunk(cXPos);
            }

            Chunk cZNeg = null;
            world.chunks.TryGetValue(new Vector2(toPrepareChunk.gridX, toPrepareChunk.gridZ - 1), out cZNeg);
            if (cZNeg != null)
            {
                GenerateRenderMeshForChunk(cZNeg);
            }

            Chunk cZPos = null;
            world.chunks.TryGetValue(new Vector2(toPrepareChunk.gridX, toPrepareChunk.gridZ + 1), out cZPos);
            if (cZPos != null)
            {
                GenerateRenderMeshForChunk(cZPos);
            }
        }

        private void GenerateRenderMeshForSection(Section toProcessSection)
        {

        }

        private void GenerateRenderMeshForChunk(Chunk chunk)
        {
            activeCurrentChunk = chunk;

            Chunk cXNeg = null;
            world.chunks.TryGetValue(new Vector2(chunk.gridX - 1, chunk.gridZ), out cXNeg);

            Chunk cXPos = null;
            world.chunks.TryGetValue(new Vector2(chunk.gridX + 1, chunk.gridZ), out cXPos);

            Chunk cZNeg = null;
            world.chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ - 1), out cZNeg);

            Chunk cZPos = null;
            world.chunks.TryGetValue(new Vector2(chunk.gridX, chunk.gridZ + 1), out cZPos);

            for (int i = 0; i < chunk.sections.Length; i++)
            { 
                Section section = chunk.sections[i];
                if(section == null)
                {
                    continue;
                }

                for (int x = 0; x < Constants.CHUNK_SIZE; x++)
                {
                    for (int z = 0; z < Constants.CHUNK_SIZE; z++)
                    {
                        int y2 = 0;
                        for(int y = 0; y < Constants.SECTION_HEIGHT; y++)
                        {
                            sbyte? b = section.blocks[x, y, z];
                            if(b == null)
                            {
                                continue;
                            }
                            BlockType block =(BlockType)b;
                            float[] tCoords = null;
                            blockDatabase.blockTextures.TryGetValue(block, out tCoords);
                            if (tCoords != null)
                            {
                                y2 = y + i * Constants.SECTION_HEIGHT;
                                if (ShouldAddEastFaceOfBlock(cXPos, section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Right, x, y2, z, tCoords, sideZLight);
                                }
                                if (ShouldAddWestFaceOfBlock(cXNeg, section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Left, x, y2, z, tCoords, sideXLight);
                                }
                                if (ShouldAddSouthFaceOfBlock(cZNeg, section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Back, x, y2, z, tCoords, sideXLight);
                                }
                                if(ShouldAddNorthFaceOfBlock(cZPos, section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Front, x, y2, z, tCoords, sideZLight);
                                }
                                if(ShouldAddTopFaceOfBlock(section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Top, x, y2, z, tCoords, topLight);
                                }
                                if(ShouldAddBottomFaceOfBlock(section, x, y, z, tCoords))
                                {
                                    AddFace(BlockSide.Bottom, x, y2, z, tCoords, bottomLight);
                                }
                            }
                            y2 = 0;
                        }
                    }
                }
            }

            Model hardBlocksChunkModel = new Model(positions.ToArray(), textureCoords.ToArray(), indices.ToArray(), lights.ToArray());
            RenderChunk newRenderChunk = new RenderChunk(hardBlocksChunkModel, chunk.gridX, chunk.gridZ);
            Vector2 chunkPos = new Vector2(chunk.gridX, chunk.gridZ);
            if (world.renderChunks.ContainsKey(chunkPos))
            {
                world.renderChunks[chunkPos] = newRenderChunk;
            }
            else
            {
                world.renderChunks.Add(chunkPos, newRenderChunk);
            }

            ResetData();
        }

        private void AddFace(BlockSide blockSide, float x, float y, float z, float[] textureCoordinates, float lightValue)
        {
            FillTextureCoordinates(blockSide, textureCoordinates);

            float[] pos = Cube.GetCubeVerticesForSide(blockSide, x, y, z);
            foreach (float f in pos)
            {
                positions.Add(f);
            }
            indices.Add(indCount);
            indices.Add(indCount + 1);
            indices.Add(indCount + 2);
            indices.Add(indCount + 2);
            indices.Add(indCount + 3);
            indices.Add(indCount);
            indCount += 4;

            lights.Add(lightValue);
            lights.Add(lightValue);
            lights.Add(lightValue);
            lights.Add(lightValue);
        }

        private bool ShouldAddWestFaceOfBlock(Chunk westChunk, Section currentSection, float x, float y, float z, float[] textureCoordinates)
        {
            if(x - 1 < 0)
            {
                Section westSection = null;
                if(westChunk != null)
                {
                    westSection = westChunk.sections[currentSection.height];
                }

                if(westSection != null)
                {
                    if(westSection.blocks[Constants.CHUNK_SIZE - 1, (int)y, (int)z] == null)
                    {
                        return true;
                    }
                }else
                {
                    return true;
                }
            }else if(currentSection.blocks[(int)x - 1, (int)y, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddEastFaceOfBlock(Chunk eastChunk, Section currentSection, float x, float y, float z, float[] textureCoordinates)
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
                }
                else
                {
                    return true;
                }
            }
            else if (currentSection.blocks[(int)x + 1, (int)y, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddNorthFaceOfBlock(Chunk northChunk, Section currentSection, float x, float y, float z, float[] textureCoordinates)
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
                }
                else
                {
                    return true;
                }
            }
            else if (currentSection.blocks[(int)x, (int)y, (int)z + 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddSouthFaceOfBlock(Chunk southChunk, Section currentSection, float x, float y, float z, float[] textureCoordinates)
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
                }
                else
                {
                    return true;
                }
            }
            else if (currentSection.blocks[(int)x, (int)y, (int)z - 1] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddTopFaceOfBlock(Section currentSection, float x, float y, float z, float[] textureCoordinates)
        {
            if(y + 1 >= Constants.SECTION_HEIGHT)
            {
                if(currentSection.height == Constants.SECTION_HEIGHT - 1)
                {
                    return true;
                }

                Section sectionAbove = activeCurrentChunk.sections[currentSection.height + 1];
                if ((sectionAbove != null && sectionAbove.blocks[(int)x, 0, (int)z] == null) || sectionAbove == null)
                {
                    return true;
                }
            }
            else if (currentSection.blocks[(int)x, (int)y + 1, (int)z] == null)
            {
                return true;
            }
            return false;
        }

        private bool ShouldAddBottomFaceOfBlock(Section currentSection, float x, float y, float z, float[] textureCoordinates)
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
            }
            else if (currentSection.blocks[(int)x, (int)y - 1, (int)z] == null)
            {
                return true;       
            }
            return false;
        }
       
        public void FillTextureCoordinates(BlockSide side, float[] tCoords)
        {
            int b = (int)side;
            textureCoords.Add(tCoords[b * 8]);
            textureCoords.Add(tCoords[b * 8 + 1]);
            textureCoords.Add(tCoords[b * 8 + 2]);
            textureCoords.Add(tCoords[b * 8 + 3]);
            textureCoords.Add(tCoords[b * 8 + 4]);
            textureCoords.Add(tCoords[b * 8 + 5]);
            textureCoords.Add(tCoords[b * 8 + 6]);
            textureCoords.Add(tCoords[b * 8 + 7]);
        }
    }
}
