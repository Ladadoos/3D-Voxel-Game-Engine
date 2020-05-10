namespace Minecraft
{
    class RemoveBlockPacket : Packet
    {
        public int BlockCount { get; private set; }
        public Vector3i[] BlockPositions { get; private set; }

        public RemoveBlockPacket(Vector3i[] blockPositions) : base(PacketType.RemoveBlock)
        {
            BlockPositions = blockPositions;
            BlockCount = blockPositions.Length;
        }

        public RemoveBlockPacket(Vector3i blockPos) : base(PacketType.RemoveBlock)
        {
            BlockPositions = new Vector3i[]{ blockPos };
            BlockCount = 1;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessRemoveBlockPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(BlockCount);
            foreach(Vector3i blockPos in BlockPositions)
            {
                bufferedStream.WriteVector3i(blockPos);
            }
        }
    }
}
