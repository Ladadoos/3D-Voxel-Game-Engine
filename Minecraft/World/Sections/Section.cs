namespace Minecraft
{
    class Section
    {  
        private BlockState[] blocks = new BlockState[16 * 16 * 16];
        public byte Height { get; private set; }
        public int GridX { get; private set; }
        public int GridZ { get; private set; }

        public Section(int gridX, int gridZ, byte height)
        {
            GridX = gridX;
            GridZ = gridZ;
            Height = height;
        }

        public void AddBlockAt(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[(localX << 8) + (localY << 4) + localZ] = blockstate;
        }

        public void RemoveBlockAt(int localX, int localY, int localZ)
        {
            blocks[(localX << 8) + (localY << 4) + localZ] = null;
        }

        public BlockState GetBlockAt(int localX, int localY, int localZ)
        {
            return blocks[(localX << 8) + (localY << 4) + localZ];
        }
    }
}
