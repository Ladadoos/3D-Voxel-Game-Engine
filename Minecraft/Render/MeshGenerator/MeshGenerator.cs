using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class MeshGenerator
    {
        protected List<float> vertexPositions = new List<float>();
        protected List<float> textureUVs = new List<float>();
        protected List<float> illumations = new List<float>();
        protected List<float> normals = new List<float>();
        protected int indicesCount;

        protected BlockModelRegistry blockModelRegistry;

        public MeshGenerator(BlockModelRegistry blockModelRegistry)
        {
            this.blockModelRegistry = blockModelRegistry;
        }

        protected void ClearData()
        {
            vertexPositions.Clear();
            textureUVs.Clear();
            illumations.Clear();
            normals.Clear();
            indicesCount = 0;
        }

        public Model GenerateMeshFor(World world, Chunk chunk)
        {
            Model chunkModel = GenerateMesh(world, chunk);
            ClearData();
            return chunkModel;
        }

        protected abstract Model GenerateMesh(World world, Chunk chunk);

        protected void AddFacesToMeshFromFront(BlockFace[] toAddFaces, Vector3 blockPos, float illumination)
        {
            foreach (BlockFace face in toAddFaces)
            {
                foreach (float uv in face.textureCoords)
                {
                    textureUVs.Add(uv);
                }

                foreach (Vector3 modelSpacePosition in face.positions)
                {
                    Vector3 world = modelSpacePosition + blockPos;
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(face.normal.X);
                    normals.Add(face.normal.Y);
                    normals.Add(face.normal.Z);
                }
            }
        }

        protected void AddFacesToMeshFromBack(BlockFace[] toAddFaces, Vector3 blockPos, float illumination)
        {
            foreach (BlockFace face in toAddFaces)
            {
                for (int i = 0; i < face.textureCoords.Length; i += 4)
                {
                    textureUVs.Add(face.textureCoords[i + 2]);
                    textureUVs.Add(face.textureCoords[i + 3]);
                    textureUVs.Add(face.textureCoords[i]);
                    textureUVs.Add(face.textureCoords[i + 1]);
                }

                for (int i = 0; i < face.positions.Length; i += 2)
                {
                    Vector3 world = face.positions[i + 1] + blockPos;
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);

                    world = face.positions[i] + blockPos;
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);
                illumations.Add(illumination);

                for (int i = 0; i < 4; i++)
                {
                    normals.Add(face.normal.X);
                    normals.Add(face.normal.Y);
                    normals.Add(face.normal.Z);
                }
            }
        }

        protected void AddFacesToMeshDualSided(BlockFace[] toAddFaces, Vector3 blockPos, float illumination)
        {
            AddFacesToMeshFromFront(toAddFaces, blockPos, illumination);
            AddFacesToMeshFromBack(toAddFaces, blockPos, illumination);
        }
    }
}
