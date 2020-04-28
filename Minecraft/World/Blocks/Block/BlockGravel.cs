namespace Minecraft
{
    class BlockGravel : Block
    {
        public BlockGravel(ushort id) : base(id)
        {
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateGravel();
        }
    }
}
