namespace Minecraft
{
    class PlaceBlockPacket : Packet
    {
        private BlockState state;

        public PlaceBlockPacket(BlockState state) : base(PacketType.BlockPlaced)
        {
            this.state = state;
        }

        public override void Execute(Game game)
        {
            game.world.AddBlockToWorld(state.position, state);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            state.ToStream(bufferedStream);
        }

        public override string ToString()
        {
            return "[" + GetType() + "] -> " + state.GetBlock().GetType() + " at " + state.position;
        }
    }
}
