namespace Minecraft
{
    class BlockStone : Block
    {
        public BlockStone(int id) : base(id)
        {
            isInteractable = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }

        public override void OnInteract(BlockState blockstate, World world)
        {
            BlockStateStone state = (BlockStateStone)blockstate;
            Logger.Info("Interacted with stone at " + state.position);
        }
    }
}
