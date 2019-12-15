using OpenTK;

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

        public void AddBlock(int localX, int worldY, int localZ, BlockState blockstate)
        {
            if (worldY < 0)
            {
                worldY = 0;
            } else if (worldY > Constants.MAX_BUILD_HEIGHT - 1)
            {
                worldY = Constants.MAX_BUILD_HEIGHT - 1;
            }

            int sectionHeight = worldY / Constants.SECTION_HEIGHT;
            if(sections[sectionHeight] == null)
            {
                sections[sectionHeight] = new Section((byte)sectionHeight);
            }

            int sectionLocalY = worldY - sectionHeight * Constants.SECTION_HEIGHT;
            if(blockstate.block == Blocks.Air)
            {
                sections[sectionHeight].RemoveBlock(localX, sectionLocalY, localZ);
            }
            else
            {
                blockstate.position = new Vector3(localX + gridX * 16, worldY, localZ + gridZ * 16);
                sections[sectionHeight].AddBlock(localX, sectionLocalY, localZ, blockstate);
            }         
        }
    }
}
