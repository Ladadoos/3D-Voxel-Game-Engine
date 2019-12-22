namespace Minecraft
{ 
    class BlockDirt : Block
    {
        public BlockDirt(int id) : base(id) { }

        public override BlockState GetNewDefaultState()
        {
            return new BlockStateDirt();
        }
    }
}
