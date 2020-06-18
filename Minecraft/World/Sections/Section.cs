namespace Minecraft
{
    class Section
    {  
        private BlockState[] blocks = new BlockState[16 * 16 * 16];
        public byte Height { get; private set; }
        public bool IsFullTransparent { get; private set; }
        public bool IsEmpty { get; private set; }
        public int GridX { get; private set; }
        public int GridZ { get; private set; }
        private int numberOfOpaqueBlocks;
        private int numberOfBlocks;

        public Section(int gridX, int gridZ, byte height)
        {
            GridX = gridX;
            GridZ = gridZ;
            Height = height;
            IsFullTransparent = true;
        }

        public void AddBlockAt(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[(localX << 8) + (localY << 4) + localZ] = blockstate;
            if(blockstate.GetBlock().IsOpaque)
                numberOfOpaqueBlocks++;
            IsFullTransparent = numberOfOpaqueBlocks == 0;

            numberOfBlocks++;
            IsEmpty = numberOfBlocks == 0;
        }

        public void RemoveBlockAt(int localX, int localY, int localZ)
        {
            int index = (localX << 8) + (localY << 4) + localZ;
            if(blocks[index] != null && blocks[index].GetBlock().IsOpaque)
                numberOfOpaqueBlocks--;
            IsFullTransparent = numberOfOpaqueBlocks == 0;

            numberOfBlocks--;
            IsEmpty = numberOfBlocks == 0;
            blocks[index] = null;
        }

        public BlockState GetBlockAt(int localX, int localY, int localZ)
        {
            return blocks[(localX << 8) + (localY << 4) + localZ];
        }
    }
}
