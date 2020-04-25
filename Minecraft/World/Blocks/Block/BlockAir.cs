﻿namespace Minecraft
{
    class BlockAir : Block
    {
        public BlockAir(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateAir();
        }

        public override AxisAlignedBox[] GetCollisionBox(BlockState state, Vector3i blockPos)
        {
            return emptyAABB;
        }
    }
}