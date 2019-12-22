namespace Minecraft
{
    class ChunkDataPacket : Packet
    {
        private Chunk chunk;

        public ChunkDataPacket(Chunk chunk) : base(PacketType.ChunkLoaded)
        {     
            this.chunk = chunk;
        }

        public override void Execute(Game game)
        {
            game.world.LoadChunk(chunk);
        }

        protected override void ToStream(NetBufferedStream bufferedStream)
        {
            chunk.ToStream(bufferedStream);
        }
    }
}
