namespace Minecraft
{
    class Section
    {  
        public sbyte height;
        //THIS SHOULD IDEALLY NOT BE NULL....
        public BlockState[,,] blocks = new BlockState[Constants.CHUNK_SIZE, Constants.SECTION_HEIGHT, Constants.CHUNK_SIZE];

        public Section(sbyte height)
        {
            this.height = height;
        }

        public void AddBlock(int localX, int localY, int localZ, BlockState blockstate)
        {
            blocks[localX, localY, localZ] = blockstate;
        }

        public void RemoveBlock(int localX, int localY, int localZ)
        {
            blocks[localX, localY, localZ] = null;
        }
    }
}
