using OpenTK;

namespace Minecraft
{
    class FaunaMeshGenerator : MeshGenerator
    {
        public FaunaMeshGenerator(BlockModelRegistry blockModelRegistry) : base(blockModelRegistry)
        {
            materialToProcess = BlockMaterial.Fauna;
        }

        protected override Model GenerateMesh(World world, Chunk chunk)
        {
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
                            if (state == null || !ShouldProcessLayer(state.block.material))
                            {
                                continue;
                            }

                            if (!blockModelRegistry.models.TryGetValue(state.block, out BlockModel blockModel))
                            {
                                throw new System.Exception("Could not find model for: " + state.block.GetType());
                            }

                            BlockFace[] faces = blockModel.GetAlwaysVisibleFaces(state);
                            AddFacesToMesh(faces, state, 1);
                        }
                    }
                }
            }

            return new Model(vertexPositions.ToArray(), textureUVs.ToArray(), illumations.ToArray(), indicesCount);
        }
    }
}
