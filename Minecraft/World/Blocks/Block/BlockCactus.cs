namespace Minecraft
{
    class BlockCactus : Block
    {
        public BlockCactus(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateCactus();
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

        public override void OnDestroy(BlockState blockState, World world, Vector3i blockPos)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            while(world.GetBlockAt(blockPos.Up()).GetBlock() == Blocks.Cactus)
            {
                world.QueueToRemoveBlockAt(blockPos.Up());
                blockPos = blockPos.Up();
            }
        }
    }
}
