namespace Minecraft
{
    class Chunk
    {
        public Section[] sections = new Section[Constants.SECTIONS_IN_CHUNKS];
        public int gridX;
        public int gridZ;

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public void AddBlock(int x, int y, int z, BlockType block)
        {
            if (y < 0)
            {
                y = 0;
            } else if (y > Constants.MAX_BUILD_HEIGHT - 1)
            {
                y = Constants.MAX_BUILD_HEIGHT - 1;
            }

            int h = y / Constants.SECTION_HEIGHT;
            if(sections[h] == null)
            {
                sections[h] = new Section((sbyte)h);
            }

            int localY = y - h * Constants.SECTION_HEIGHT;
            if(block == BlockType.Air)
            {
                sections[h].RemoveBlock(x, localY, z);
            }
            else
            {
                sections[h].AddBlock(x, localY, z, block);
            }
            
        }
    }
}
