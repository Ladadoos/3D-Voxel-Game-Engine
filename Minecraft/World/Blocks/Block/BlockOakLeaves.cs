namespace Minecraft
{
    class BlockOakLeaves : Block
    {
        public BlockOakLeaves(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateOakLeaves();
        }
    }
}
