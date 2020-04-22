namespace Minecraft
{
    class BlockStone : Block
    {
        public BlockStone(ushort id) : base(id)
        {
            isInteractable = true;
        }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateStone();
        }

        public override void OnInteract(BlockState blockstate, Vector3i blockPos, World world)
        {
            BlockStateStone state = (BlockStateStone)blockstate;
            Logger.Info("Interacted with stone");
        }
    }
}
