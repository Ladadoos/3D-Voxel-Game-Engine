using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class Model
    {
        public int vaoId;
        public int indicesCount;

        public List<int> buffers = new List<int>();

        public Model(float[] positions, float[] textureCoordinates, float[] lights, int indicesCount)
        {
            CreateData(positions, textureCoordinates, lights, indicesCount);
        }

        private void CreateData(float[] positions, float[] textureCoordinates, float[] lights, int indicesCount)
        {
            this.indicesCount = indicesCount;

            CreateVAO();
            Bind();

            CreateVBO(3, positions);
            CreateVBO(2, textureCoordinates);
            CreateVBO(1, lights);
            //CreateIBO(indices);

            Unbind();
        }

        public Model(float[] positions, int[] indices)
        {
            indicesCount = indices.Length;

            CreateVAO();
            Bind();

            CreateVBO(3, positions);
            CreateIBO(indices);

            Unbind();
        }

        public void CleanUp()
        {
            foreach (int buffer in buffers)
            {
                GL.DeleteTexture(buffer);
            }
            GL.DeleteVertexArray(vaoId);
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
        private void CreateVBO(int nrOfElementsInStructure, float[] data)
        {
            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(buffers.Count, nrOfElementsInStructure, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.EnableVertexAttribArray(buffers.Count);
            buffers.Add(vboID);
        }

        /// <summary> Creates a vertex array object </summary>
        private void CreateVAO()
        {
            vaoId = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(vaoId);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
        }
    }
}
