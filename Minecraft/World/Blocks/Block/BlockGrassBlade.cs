namespace Minecraft
{
    class BlockGrassBlade : Block
    {
        public BlockGrassBlade(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateGrassBlade();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }
    }
}
