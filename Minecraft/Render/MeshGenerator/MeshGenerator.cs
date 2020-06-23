using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    abstract class MeshGenerator
    {
        protected float[] vertexPositions = new float[1048576];
        protected int positionPointer = 0;
        protected float[] vertexUVs = new float[1048576];
        protected int uvsPointer = 0;
        protected uint[] vertexLights = new uint[1048576];
        protected int lightsPointer = 0;
        protected float[] vertexNormals = new float[1048576];
        protected int normalPointer = 0;
        protected int indicesCount;

        protected readonly BlockModelRegistry blockModelRegistry;

        protected MeshGenerator(BlockModelRegistry blockModelRegistry)
        {
            this.blockModelRegistry = blockModelRegistry;
        }

        protected void ClearData()
        {
            positionPointer = 0;
            uvsPointer = 0;
            lightsPointer = 0;
            normalPointer = 0;
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
            vertexPositions[positionPointer++] = vec.X;
            vertexPositions[positionPointer++] = vec.Y;
            vertexPositions[positionPointer++] = vec.Z;
        }

        private void AddVector2(Vector2 vec)
        {
            vertexUVs[uvsPointer++] = vec.X;
            vertexUVs[uvsPointer++] = vec.Y;
        }

        protected void AddFacesToMeshFromFront(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights, bool flip)
        {
            foreach(BlockFace face in toAddFaces)
                AddFace(face, blockPos, lights, flip, 0, 1, 2, 0, 2, 3);
        }

        protected void AddFacesToMeshFromBack(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights, bool flip)
        {
            foreach(BlockFace face in toAddFaces)
                AddFace(face, blockPos, lights, flip, 1, 0, 3, 1, 3, 2);
        }

        private void AddFace(BlockFace face, Vector3i blockPos, Light[] lights, bool flip, 
            int v1, int v2, int v3, int v4, int v5, int v6)
        {
            if(!flip)
            {
                AddVector2(face.TextureCoords[v1]);
                AddVector2(face.TextureCoords[v2]);
                AddVector2(face.TextureCoords[v3]);
                AddVector2(face.TextureCoords[v4]);
                AddVector2(face.TextureCoords[v5]);
                AddVector2(face.TextureCoords[v6]);
            } else
            {
                AddVector2(face.TextureCoords[v1]);
                AddVector2(face.TextureCoords[v2]);
                AddVector2(face.TextureCoords[v6]);
                AddVector2(face.TextureCoords[v2]);
                AddVector2(face.TextureCoords[v3]);
                AddVector2(face.TextureCoords[v6]);
            }


            if(!flip)
            {
                AddVector3(face.Positions[v1].Plus(blockPos));
                AddVector3(face.Positions[v2].Plus(blockPos));
                AddVector3(face.Positions[v3].Plus(blockPos));
                AddVector3(face.Positions[v4].Plus(blockPos));
                AddVector3(face.Positions[v5].Plus(blockPos));
                AddVector3(face.Positions[v6].Plus(blockPos));
            } else
            {
                AddVector3(face.Positions[v1].Plus(blockPos));
                AddVector3(face.Positions[v2].Plus(blockPos));
                AddVector3(face.Positions[v6].Plus(blockPos));
                AddVector3(face.Positions[v2].Plus(blockPos));
                AddVector3(face.Positions[v3].Plus(blockPos));
                AddVector3(face.Positions[v6].Plus(blockPos));
            }

            indicesCount += 6;
            if(face.Positions.Length != lights.Length)
                throw new System.Exception();

            if(!flip)
            {
                vertexLights[lightsPointer++] = lights[v1].GetStorage();
                vertexLights[lightsPointer++] = lights[v2].GetStorage();
                vertexLights[lightsPointer++] = lights[v3].GetStorage();
                vertexLights[lightsPointer++] = lights[v4].GetStorage();
                vertexLights[lightsPointer++] = lights[v5].GetStorage();
                vertexLights[lightsPointer++] = lights[v6].GetStorage();
            } else
            {
                vertexLights[lightsPointer++] = lights[v1].GetStorage();
                vertexLights[lightsPointer++] = lights[v2].GetStorage();
                vertexLights[lightsPointer++] = lights[v6].GetStorage();
                vertexLights[lightsPointer++] = lights[v2].GetStorage();
                vertexLights[lightsPointer++] = lights[v3].GetStorage();
                vertexLights[lightsPointer++] = lights[v6].GetStorage();

            }

            vertexNormals[normalPointer++] = face.Normal.X;
            vertexNormals[normalPointer++] = face.Normal.Y;
            vertexNormals[normalPointer++] = face.Normal.Z;
        }

        protected void AddFacesToMeshDualSided(BlockFace[] toAddFaces, Vector3i blockPos, Light[] lights, bool flip)
        {
            AddFacesToMeshFromFront(toAddFaces, blockPos, lights, flip);
            AddFacesToMeshFromBack(toAddFaces, blockPos, lights, flip);
        }
    }
}
