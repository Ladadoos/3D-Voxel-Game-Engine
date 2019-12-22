using OpenTK;

namespace Minecraft
{
    class BlockFlower : Block
    {
        public BlockFlower(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateFlower();
        }

        public override AABB[] GetCollisionBox(BlockState state)
        {
            return emptyAABB;
        }

        public override bool CanAddBlockAt(World world, Vector3 intPosition)
        {
            return world.GetBlockAt(intPosition + new Vector3(0, -1, 0)).GetBlock() == Blocks.Dirt;
        }
    }
}
