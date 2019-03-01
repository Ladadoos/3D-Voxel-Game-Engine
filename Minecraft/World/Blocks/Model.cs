using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

namespace Minecraft.World.Blocks
{
    class Model
    {
        public int vaoId;
        public int indicesCount;

        public List<int> buffers = new List<int>();

        public Model(float[] positions, float[] textureCoordinates, int[] indices, float[] lights)
        {
            CreateData(positions, textureCoordinates, indices, lights);
        }

        public void AddData(float[] positions, float[] textureCoordinates, int[] indices, float[] lights)
        {
            DeleteData();
            CreateData(positions, textureCoordinates, indices, lights);
        }

        private void CreateData(float[] positions, float[] textureCoordinates, int[] indices, float[] lights)
        {
            indicesCount = indices.Length;
            CreateVAO();

            AddVBO(3, positions);
            AddVBO(2, textureCoordinates);
            AddVBO(1, lights);
            AddEBO(indices);

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

        private void DeleteData()
        {
            CleanUp();
            vaoId = 0;
            indicesCount = 0;
            buffers = new List<int>();
        }

        private void AddEBO(int[] indices)
        {
            int vboID = GL.GenBuffer();
            buffers.Add(vboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);
        }

        private void AddVBO(int coordinateSize, float[] data)
        {
            int vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(data.Length * sizeof(float)), data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(buffers.Count, coordinateSize, VertexAttribPointerType.Float, false, 0, 0);
            // GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.EnableVertexAttribArray(buffers.Count);
            buffers.Add(vboID);
        }

        private void CreateVAO()
        {
            vaoId = GL.GenVertexArray();
            Bind();
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
