namespace Minecraft
{
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
