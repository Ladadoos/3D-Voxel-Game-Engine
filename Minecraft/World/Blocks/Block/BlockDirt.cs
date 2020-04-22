namespace Minecraft
{ 
    class BlockDirt : Block
    {
        public BlockDirt(ushort id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDirt();
        }
    }
}
