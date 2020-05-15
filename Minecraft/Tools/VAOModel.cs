using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class VAOModel
    {
        public int IndicesCount { get; private set; }
        private int vaoId;
        private readonly List<int> buffers = new List<int>();

        public VAOModel(float[] positions, float[] textureCoordinates, float[] lights, float[] normals, int indicesCount)
        {
            IndicesCount = indicesCount;
            CreateVAO();
            BindVAO();
            CreateVBO(3, positions);
            CreateVBO(3, normals);
            CreateVBO(2, textureCoordinates);
            CreateVBO(1, lights);
            UnbindVAO();
        }

        public VAOModel(ChunkBufferLayout chunkLayout)
        {
            IndicesCount = chunkLayout.indicesCount;
            CreateVAO();
            BindVAO();
            CreateVBO(3, chunkLayout.positions);
            CreateVBO(3, chunkLayout.normals);
            CreateVBO(2, chunkLayout.textureCoordinates);
            CreateVBO(1, chunkLayout.lights);
            UnbindVAO();
        }

        public VAOModel(float[] positions, int[] indices)
        {
            IndicesCount = indices.Length;
            CreateVAO();
            BindVAO();
            CreateVBO(3, positions);
            CreateIBO(indices);
            UnbindVAO();
        }

        public VAOModel(float[] positions, float[] textureCoordinates, int indicesCount)
        {
            IndicesCount = indicesCount;
            CreateVAO();
            BindVAO();
            CreateVBO(3, positions);
            CreateVBO(2, textureCoordinates);
            UnbindVAO();
        }

        public void CleanUp()
        {
            foreach (int buffer in buffers)
            {
                GL.DeleteBuffer(buffer);
            }
            GL.DeleteVertexArray(vaoId);
            buffers.Clear();
        }

        /// <summary> Creates an index buffer object and buffers the given indices. </summary>
        private void CreateIBO(int[] indices)
        {
            int vboID = GL.GenBuffer();
            buffers.Add(vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Creates a vertex bufffer object and buffers the given float values. The integer specifies the number of elements in the datastructure.
        /// A Vector3 would for example have this integer set to 3 (X, Y, Z)
        /// </summary>
        private void CreateVBO<T>(int nrOfElementsInStructure, T[] data) where T : struct
        {
            VertexAttribPointerType dataType = VertexAttribPointerType.Float;
            if(typeof(T) == typeof(Light) || typeof(T) == typeof(uint))
            {
                dataType = VertexAttribPointerType.Float;
            }

            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * SizeOf<T>()), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(buffers.Count, nrOfElementsInStructure, dataType, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.EnableVertexAttribArray(buffers.Count);
            buffers.Add(vboID);
        }

        /// <summary> Creates a vertex array object </summary>
        private void CreateVAO()
        {
            vaoId = GL.GenVertexArray();
        }

        public void BindVAO()
        {
            GL.BindVertexArray(vaoId);
        }

        public void UnbindVAO()
        {
            GL.BindVertexArray(0);
        }

        private int SizeOf<T>() where T : struct
        {
            return Marshal.SizeOf(default(T));
        }
    }
}
