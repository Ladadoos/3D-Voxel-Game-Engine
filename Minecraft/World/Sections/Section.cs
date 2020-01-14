using System;

namespace Minecraft
{
    [Serializable]
    class Section
    {  
        public byte height;
        public BlockState[,,] blocks = new BlockState[Constants.CHUNK_SIZE, Constants.SECTION_HEIGHT, Constants.CHUNK_SIZE];
        public int gridX;
        public int gridZ;

        public Section(int gridX, int gridZ, byte height)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
            this.height = height;
        }

        public Section DeepCopy()
        {
            Section newSection = new Section(gridX, gridZ, height);
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        newSection.blocks[x, y, z] = blocks[x, y, z]?.ShallowCopy();
                    }
                }
            }
            return newSection;
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
