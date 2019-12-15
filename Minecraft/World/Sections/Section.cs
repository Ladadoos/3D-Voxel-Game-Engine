namespace Minecraft
{
    class Section
    {  
        public byte height;
        public BlockState[,,] blocks = new BlockState[Constants.CHUNK_SIZE, Constants.SECTION_HEIGHT, Constants.CHUNK_SIZE];

        public Section(byte height)
        {
            this.height = height;
        }

        public void AddBlock(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[localX, localY, localZ] = blockstate;
        }

        //Consider optimalization where you completely ignore a section if its fully opaque and surrounding section walls are full opaque
        public void RemoveBlock(int localX, int localY, int localZ)
        {
            blocks[localX, localY, localZ] = null;
        }
    }
}
