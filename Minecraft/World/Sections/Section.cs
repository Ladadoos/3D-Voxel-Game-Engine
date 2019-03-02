using Minecraft.World.Blocks;

namespace Minecraft.World.Sections
{
    class Section
    {  
        public sbyte height;
        public sbyte?[,,] blocks = new sbyte?[Constants.CHUNK_SIZE, Constants.SECTION_HEIGHT, Constants.CHUNK_SIZE];

        public Section(sbyte height)
        {
            this.height = height;
        }

        public void AddBlock(int x, int y, int z, BlockType block)
        {
            blocks[x, y, z] = (sbyte)block;
        }

        public void RemoveBlock(int x, int y, int z)
        {
            blocks[x, y, z] = null;
        }
    }
}
