using OpenTK;

namespace Minecraft
{
    class BlockFlower : Block
    {
        public BlockFlower(ushort id) : base(id)
        {
            IsOpaque = false;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateFlower();
        }

        public override AxisAlignedBox[] GetSelectionBox(BlockState state, Vector3i blockPos)
        {
            return new AxisAlignedBox[] {
                new AxisAlignedBox(
                    new Vector3(blockPos.X, blockPos.Y, blockPos.Z) + new Vector3(0.25F, 0, 0.25F),
                    new Vector3(blockPos.X + Constants.CUBE_DIM, blockPos.Y + Constants.CUBE_DIM, blockPos.Z + Constants.CUBE_DIM) - new Vector3(0.25F, 0.25f, 0.25F))
            };
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
