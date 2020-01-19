namespace Minecraft
{
    class BlockGrass : Block
    {
        public BlockGrass(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateGrass();
        }
    }
}
