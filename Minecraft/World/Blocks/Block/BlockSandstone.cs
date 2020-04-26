namespace Minecraft
{
    class BlockSandstone : Block
    {
        public BlockSandstone(ushort id) : base(id)
        {
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateSandStone();
        }
    }
}
