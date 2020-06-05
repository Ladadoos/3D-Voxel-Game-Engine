using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class MeshGenerator
    {
        protected List<float> vertexPositions = new List<float>();
        protected List<float> textureUVs = new List<float>();
        protected List<uint> illuminations = new List<uint>();
        protected List<float> normals = new List<float>();
        protected int indicesCount;

        protected readonly BlockModelRegistry blockModelRegistry;

        protected MeshGenerator(BlockModelRegistry blockModelRegistry)
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

        protected void AddFacesToMeshFromFront(BlockFace[] toAddFaces, Vector3i blockPos, Light light)
        {
            foreach (BlockFace face in toAddFaces)
            {
                foreach (float uv in face.TextureCoords)
                {
                    textureUVs.Add(uv);
                }

                foreach (Vector3 modelSpacePosition in face.Positions)
                {
                    Vector3 world = modelSpacePosition.Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                for(int i = 0; i < face.Positions.Length; i++)
                {
                    illuminations.Add(light.GetStorage());
                }

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(face.Normal.X);
                    normals.Add(face.Normal.Y);
                    normals.Add(face.Normal.Z);
                }
            }
        }

        protected void AddFacesToMeshFromBack(BlockFace[] toAddFaces, Vector3i blockPos, Light light)
        {
            foreach (BlockFace face in toAddFaces)
            {
                for (int i = 0; i < face.TextureCoords.Length; i += 4)
                {
                    textureUVs.Add(face.TextureCoords[i + 2]);
                    textureUVs.Add(face.TextureCoords[i + 3]);
                    textureUVs.Add(face.TextureCoords[i]);
                    textureUVs.Add(face.TextureCoords[i + 1]);
                }

                for (int i = 0; i < face.Positions.Length; i += 2)
                {
                    Vector3 world = face.Positions[i + 1].Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);

                    world = face.Positions[i].Plus(blockPos);
                    vertexPositions.Add(world.X);
                    vertexPositions.Add(world.Y);
                    vertexPositions.Add(world.Z);
                }

                indicesCount += 4;
                for(int i = 0; i < face.Positions.Length; i++)
                {
                    illuminations.Add(light.GetStorage());
                }

                for (int i = 0; i < 4; i++)
                {
                    normals.Add(face.Normal.X);
                    normals.Add(face.Normal.Y);
                    normals.Add(face.Normal.Z);
                }
            }
        }

        protected void AddFacesToMeshDualSided(BlockFace[] toAddFaces, Vector3i blockPos, Light light)
        {
            AddFacesToMeshFromFront(toAddFaces, blockPos, light);
            AddFacesToMeshFromBack(toAddFaces, blockPos, light);
        }
    }
}
