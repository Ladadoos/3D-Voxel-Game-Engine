using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class MeshGenerator
    {
        protected List<float> vertexPositions = new List<float>();
        protected List<float> textureUVs = new List<float>();
        protected List<float> illuminations = new List<float>();
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
            illuminations.Clear();
            normals.Clear();
            indicesCount = 0;
        }

        public ChunkBufferLayout GenerateMeshFor(World world, Chunk chunk)
        {
            ChunkBufferLayout chunkModel = GenerateMesh(world, chunk);
            ClearData();
            return chunkModel;
        }

        protected abstract ChunkBufferLayout GenerateMesh(World world, Chunk chunk);

        protected void AddFacesToMeshFromFront(BlockFace[] toAddFaces, Vector3i blockPos, float globalIllumination)
        {
            foreach (BlockFace face in toAddFaces)
            {
                foreach (float uv in face.textureCoords)
                {
                    textureUVs.Add(uv);
                }

                foreach (Vector3 modelSpacePosition in face.positions)
                {
                    Vector3 world = modelSpacePosition.Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                foreach(float illum in face.illumination)
                {
                    illuminations.Add(illum * globalIllumination);
                }

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(face.normal.X);
                    normals.Add(face.normal.Y);
                    normals.Add(face.normal.Z);
                }
            }
        }

        protected void AddFacesToMeshFromBack(BlockFace[] toAddFaces, Vector3i blockPos, float globalIllumination)
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
                    Vector3 world = face.positions[i + 1].Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);

                    world = face.positions[i].Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                foreach (float illum in face.illumination)
                {
                    illuminations.Add(illum * globalIllumination);
                }

                for (int i = 0; i < 4; i++)
                {
                    normals.Add(face.normal.X);
                    normals.Add(face.normal.Y);
                    normals.Add(face.normal.Z);
                }
            }
        }

        protected void AddFacesToMeshDualSided(BlockFace[] toAddFaces, Vector3i blockPos, float globalIllumination)
        {
            AddFacesToMeshFromFront(toAddFaces, blockPos, globalIllumination);
            AddFacesToMeshFromBack(toAddFaces, blockPos, globalIllumination);
        }
    }
}
