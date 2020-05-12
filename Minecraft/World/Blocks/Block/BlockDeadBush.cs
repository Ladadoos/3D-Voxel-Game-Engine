namespace Minecraft
{
    class BlockDeadBush : Block
    {
        public BlockDeadBush(ushort id) : base(id)
        {
            IsOpaque = false;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDeadBush();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }

        public override bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            return world.GetBlockAt(blockPos.Down()).GetBlock() == Blocks.Sand;
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
