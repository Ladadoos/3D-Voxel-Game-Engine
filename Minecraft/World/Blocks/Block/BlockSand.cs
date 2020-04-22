namespace Minecraft
{
    class BlockSand : Block
    {
        public BlockSand(ushort id) : base(id)
        {
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateSand();
        }
    }
}
