namespace Minecraft
{
    class ChunkDataPacket : Packet
    {
        public Chunk chunk { get; private set; }

        public ChunkDataPacket(Chunk chunk) : base(PacketType.ChunkLoad)
        {     
            this.chunk = chunk;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkDataPacket(this);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            chunk.ToStream(bufferedStream);
        }
    }
}
