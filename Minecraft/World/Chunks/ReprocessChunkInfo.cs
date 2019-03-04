namespace Minecraft
{
    class ReprocessChunkInfo
    {
        public bool reprocessSuroundings;
        public Chunk toReprocessChunk;

        public ReprocessChunkInfo(Chunk toReprocessChunk, bool reprocessSuroundings)
        {
            this.toReprocessChunk = toReprocessChunk;
            this.reprocessSuroundings = reprocessSuroundings;
        }
    }
}
