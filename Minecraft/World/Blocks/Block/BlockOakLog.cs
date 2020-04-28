namespace Minecraft
{
    class BlockOakLog : Block
    {
        public BlockOakLog(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateOakLog();
        }
    }
}
