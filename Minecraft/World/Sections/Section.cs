namespace Minecraft
{
    class Section
    {  
        public BlockState[] blocks = new BlockState[Constants.CHUNK_SIZE * Constants.SECTION_HEIGHT * Constants.CHUNK_SIZE];
        public byte height { get; private set; }
        public int gridX { get; private set; }
        public int gridZ { get; private set; }

        public Section(int gridX, int gridZ, byte height)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
            this.height = height;
        }

        public void AddBlock(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[(localX << 8) + (localY << 4) + localZ] = blockstate;
        }

        public void RemoveBlock(int localX, int localY, int localZ)
        {
            blocks[(localX << 8) + (localY << 4) + localZ] = null;
        }

        public BlockState GetBlockAt(int localX, int localY, int localZ)
        {
            return blocks[(localX << 8) + (localY << 4) + localZ];
        }
    }
}
