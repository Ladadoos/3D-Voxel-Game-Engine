namespace Minecraft
{
    class BlockGrassBlade : Block
    {
        public BlockGrassBlade(ushort id) : base(id)
        {
            IsOverridable = true;
            IsOpaque = false;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateGrassBlade();
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

        public override void OnNotify(BlockState blockState, BlockState sourceBlockState, World world, Vector3i blockPos, Vector3i sourceBlockPos)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            if(blockPos == sourceBlockPos.Up() && world.GetBlockAt(sourceBlockPos).GetBlock() == Blocks.Air)
            {
                world.QueueToRemoveBlockAt(blockPos);
            }
        }
    }
}
