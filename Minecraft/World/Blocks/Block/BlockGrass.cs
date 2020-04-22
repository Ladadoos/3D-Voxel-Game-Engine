namespace Minecraft
{
    class BlockGrass : Block
    {
        public BlockGrass(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateGrass();
        }
    }
}
