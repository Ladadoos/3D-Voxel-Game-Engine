namespace Minecraft
{
    class ChunkDataRequestPacket : Packet
    {
        public int gridX { get; private set; }
        public int gridZ { get; private set; }

        public ChunkDataRequestPacket(int gridX, int gridZ) : base(PacketType.ChunkDataRequest)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkDataRequestPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(gridX);
            bufferedStream.WriteInt32(gridZ);
        }
    }
}
