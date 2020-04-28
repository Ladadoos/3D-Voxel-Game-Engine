namespace Minecraft
{
    class RemoveBlockPacket : Packet
    {
        public int blockCount { get; private set; }
        public Vector3i[] blockPositions { get; private set; }

        public RemoveBlockPacket(Vector3i[] blockPositions) : base(PacketType.RemoveBlock)
        {
            this.blockPositions = blockPositions;
            this.blockCount = blockPositions.Length;
        }

        public RemoveBlockPacket(Vector3i blockPos) : base(PacketType.RemoveBlock)
        {
            this.blockPositions = new Vector3i[]{ blockPos };
            this.blockCount = 1;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessRemoveBlockPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(blockCount);
            foreach(Vector3i blockPos in blockPositions)
            {
                bufferedStream.WriteVector3i(blockPos);
            }
        }
    }
}
