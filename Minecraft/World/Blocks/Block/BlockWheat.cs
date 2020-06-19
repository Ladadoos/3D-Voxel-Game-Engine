namespace Minecraft
{
    class BlockWheat : Block
    {
        private readonly int maxMaturity = 2;
        private readonly float secondsToGrow = 3;

        public BlockWheat(ushort id) : base(id)
        {
            IsTickable = true;
            IsOpaque = false;
            HasCustomState = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateWheat();
        }

        public override void OnTick(BlockState blockState, World world, Vector3i blockPos, float deltaTime)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            BlockStateWheat originalWheat = (BlockStateWheat)blockState;
            if(originalWheat.maturity >= maxMaturity)
            {
                return;
            }

            originalWheat.elapsedTimeSinceLastGrowth += deltaTime;
            if(originalWheat.elapsedTimeSinceLastGrowth >= secondsToGrow)
            {
                originalWheat.elapsedTimeSinceLastGrowth = 0;

                world.QueueToRemoveBlockAt(blockPos);
                BlockStateWheat newWheat = (BlockStateWheat)Blocks.GetState(Blocks.Wheat);
                newWheat.maturity = (ushort)(originalWheat.maturity + 1);
                world.QueueToAddBlockAt(blockPos, newWheat);
            }
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }

        public override bool CanAddBlockAt(World world, Vector3i blockPos)
        {
            Block block = world.GetBlockAt(blockPos.Down()).GetBlock();
            return block == Blocks.Grass;
        }

        public override void OnNotify(BlockState blockState, BlockState sourceBlockState, World world, Vector3i blockpos, Vector3i sourceBlockPos)
        {
            if(!(world is WorldServer))
            {
                return;
            }

            if(sourceBlockPos == blockpos.Down() && world.GetBlockAt(sourceBlockPos).GetBlock() == Blocks.Air)
            {
                world.QueueToRemoveBlockAt(blockpos);
            }
        }
    }
}
