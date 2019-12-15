using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    abstract class MeshGenerator
    {
        protected List<float> vertexPositions = new List<float>();
        protected List<float> textureUVs = new List<float>();
        protected List<float> illumations = new List<float>();
        protected int indicesCount;

        protected BlockModelRegistry blockModelRegistry;
        protected BlockMaterial materialToProcess;

        public MeshGenerator(BlockModelRegistry blockModelRegistry)
        {
            this.blockModelRegistry = blockModelRegistry;
        }

        protected void ClearData()
        {
            vertexPositions.Clear();
            textureUVs.Clear();
            illumations.Clear();
            indicesCount = 0;
        }

        public Model GenerateMeshFor(World world, Chunk chunk)
        {
            Model chunkModel = GenerateMesh(world, chunk);
            ClearData();
            return chunkModel;
        }

        public bool ShouldProcessLayer(BlockMaterial material)
        {
            return material == materialToProcess;
        }

        protected abstract Model GenerateMesh(World world, Chunk chunk);

        protected void AddFacesToMesh(BlockFace[] toAddFaces, BlockState sourceState, float illumination)
        {
            if(toAddFaces.Length <= 0)
            {
                return;
            }

            foreach (BlockFace face in toAddFaces)
            {
                foreach (float uv in face.textureCoords)
                {
                    textureUVs.Add(uv);
                }

                foreach (Vector3 modelSpacePosition in face.positions)
                {
                    Vector3 world = modelSpacePosition + new Vector3(sourceState.ChunkLocalPosition());
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);
            }
        }
    }
}
