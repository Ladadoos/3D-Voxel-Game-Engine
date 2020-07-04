namespace Minecraft
{
    /// <summary>
    /// Captures the output of tessellating a chunk in order to be forwarded
    /// to VBO/VAOs to be rendered.
    /// </summary>
    struct ChunkBufferLayout
    {
        public float[] VertexPositions;
        public int PositionsPointer;
        public float[] VertexUVs;
        public int UVsPointer;
        public uint[] VertexLights;
        public int LightsPointer;
        public float[] VertexNormals;
        public int NormalsPointer;
        public int IndicesCount;
    }
}
