﻿namespace Minecraft
{
    class BlockStone : Block
    {
        public BlockStone(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }

        public override bool OnInteract(BlockState blockstate, World world)
        {
            BlockStateStone state = (BlockStateStone)blockstate;
            Logger.Info("Interacted with stone at " + state.position);
            return true;
        }
    }
}