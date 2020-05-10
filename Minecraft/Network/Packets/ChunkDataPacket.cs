namespace Minecraft
{
    class ChunkDataPacket : Packet
    {
        public Chunk Chunk { get; private set; }

        public ChunkDataPacket(Chunk chunk) : base(PacketType.ChunkData)
        {     
            Chunk = chunk;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkDataPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteChunk(Chunk);
        }
    }
}
