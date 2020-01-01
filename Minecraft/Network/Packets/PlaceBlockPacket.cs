namespace Minecraft
{
    class PlaceBlockPacket : Packet
    {
        public BlockState blockState { get; private set; }
        public Vector3i blockPos { get; private set; }

        public PlaceBlockPacket(BlockState blockState, Vector3i blockPos) : base(PacketType.PlaceBlock)
        {
            this.blockState = blockState;
            this.blockPos = blockPos;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessPlaceBlockPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(blockPos.X);
            bufferedStream.WriteInt32(blockPos.Y);
            bufferedStream.WriteInt32(blockPos.Z);
            blockState.ToStream(bufferedStream);
        }

        public override string ToString()
        {
            return "[" + GetType() + "] -> " + blockState.GetBlock().GetType() + " at " + blockPos;
        }
    }
}
