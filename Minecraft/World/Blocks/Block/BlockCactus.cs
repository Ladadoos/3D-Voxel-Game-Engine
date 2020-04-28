namespace Minecraft
{
    class BlockCactus : Block
    {
        public BlockCactus(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateCactus();
        }
    }
}
