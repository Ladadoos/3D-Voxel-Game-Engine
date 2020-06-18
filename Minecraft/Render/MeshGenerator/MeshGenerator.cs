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

        private void AddVector3(Vector3 vec)
        {
            vertexPositions.Add(vec.X);
            vertexPositions.Add(vec.Y);
            vertexPositions.Add(vec.Z);
        }

        private void AddVector2(Vector2 vec)
        {
            textureUVs.Add(vec.X);
            textureUVs.Add(vec.Y);
        }

        protected void AddFacesToMeshFromFront(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights)
        {
            foreach (BlockFace face in toAddFaces)
            {
                AddVector2(face.TextureCoords[0]);
                AddVector2(face.TextureCoords[1]);
                AddVector2(face.TextureCoords[2]);
                AddVector2(face.TextureCoords[0]);
                AddVector2(face.TextureCoords[2]);
                AddVector2(face.TextureCoords[3]);

                AddVector3(face.Positions[0].Plus(blockPos));
                AddVector3(face.Positions[1].Plus(blockPos));
                AddVector3(face.Positions[2].Plus(blockPos));
                AddVector3(face.Positions[0].Plus(blockPos));
                AddVector3(face.Positions[2].Plus(blockPos));
                AddVector3(face.Positions[3].Plus(blockPos));

                indicesCount += 6;
                if(face.Positions.Length != lights.Length)
                    throw new System.Exception();

                illuminations.Add(lights[0].GetStorage());
                illuminations.Add(lights[1].GetStorage());
                illuminations.Add(lights[2].GetStorage());
                illuminations.Add(lights[0].GetStorage());
                illuminations.Add(lights[2].GetStorage());
                illuminations.Add(lights[3].GetStorage());

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(face.Normal.X);
                    normals.Add(face.Normal.Y);
                    normals.Add(face.Normal.Z);
                }
            }
        }

        protected void AddFacesToMeshFromBack(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights)
        {
            foreach(BlockFace face in toAddFaces)
            {
                AddVector2(face.TextureCoords[1]);
                AddVector2(face.TextureCoords[0]);
                AddVector2(face.TextureCoords[3]);
                AddVector2(face.TextureCoords[1]);
                AddVector2(face.TextureCoords[3]);
                AddVector2(face.TextureCoords[2]);

                AddVector3(face.Positions[1].Plus(blockPos));
                AddVector3(face.Positions[0].Plus(blockPos));
                AddVector3(face.Positions[3].Plus(blockPos));
                AddVector3(face.Positions[1].Plus(blockPos));
                AddVector3(face.Positions[3].Plus(blockPos));
                AddVector3(face.Positions[2].Plus(blockPos));

                indicesCount += 6;
                if(face.Positions.Length != lights.Length)
                    throw new System.Exception();

                illuminations.Add(lights[1].GetStorage());
                illuminations.Add(lights[0].GetStorage());
                illuminations.Add(lights[3].GetStorage());
                illuminations.Add(lights[1].GetStorage());
                illuminations.Add(lights[3].GetStorage());
                illuminations.Add(lights[2].GetStorage());

                for(int i = 0; i < 4; i++)
                {
                    normals.Add(face.Normal.X);
                    normals.Add(face.Normal.Y);
                    normals.Add(face.Normal.Z);
                }
            }
        }

        protected void AddFacesToMeshDualSided(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights)
        {
            AddFacesToMeshFromFront(toAddFaces, blockPos, lights);
            AddFacesToMeshFromBack(toAddFaces, blockPos, lights);
        }
    }
}
