namespace Minecraft
{
    class ChunkUnloadPacket : Packet
    {
        public int gridX { get; private set; }
        public int gridZ { get; private set; }

        public ChunkUnloadPacket(int gridX, int gridZ) : base(PacketType.ChunkUnload)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkUnloadPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            bufferedStream.WriteInt32(gridX);
            bufferedStream.WriteInt32(gridZ);
        }
    }
}
