namespace Minecraft
{
    class BlockDeadBush : Block
    {
        public BlockDeadBush(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDeadBush();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }
    }
}
