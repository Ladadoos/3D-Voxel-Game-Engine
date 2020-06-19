namespace Minecraft
{
    class ChunkBufferLayout
    {
        public float[] vertexPositions;
        public int positionsPointer;
        public float[] vertexUVs;
        public int uvsPointer;
        public uint[] vertexLights;
        public int lightsPointer;
        public float[] vertexNormals;
        public int normalsPointer;
        public int indicesCount;
    }
}
