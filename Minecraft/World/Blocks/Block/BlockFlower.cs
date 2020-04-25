namespace Minecraft
{
    class BlockFlower : Block
    {
        public BlockFlower(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateFlower();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }

        public override bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            Block block = world.GetBlockAt(blockPos.Down()).GetBlock();
            return block == Blocks.Dirt || block == Blocks.Grass;
        }
    }
}
