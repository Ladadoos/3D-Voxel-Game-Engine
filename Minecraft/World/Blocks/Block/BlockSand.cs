namespace Minecraft
{
    class BlockSand : Block
    {
        public BlockSand(int id) : base(id)
        {
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateSand();
        }
    }
}
