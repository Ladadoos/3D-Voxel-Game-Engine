namespace Minecraft
{
    class BlockStone : Block
    {
        public BlockStone(ushort id) : base(id)
        {
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }
    }
}
