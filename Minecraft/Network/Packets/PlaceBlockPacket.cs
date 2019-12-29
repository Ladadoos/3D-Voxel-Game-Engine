namespace Minecraft
{
    class PlaceBlockPacket : Packet
    {
        public BlockState state { get; private set; }

        public PlaceBlockPacket(BlockState state) : base(PacketType.PlaceBlock)
        {
            this.state = state;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlaceBlockPacket(this);
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
