using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class ChunkUnloadPacket : Packet
    {
        public List<Vector2> ChunkGridPositions { get; private set; }

        public ChunkUnloadPacket(List<Vector2> chunkGridPositions) : base(PacketType.ChunkUnload)
        {
            ChunkGridPositions = chunkGridPositions;
        }

        public override void Process(INetHandler netHandler)
        {
            netHandler.ProcessChunkUnloadPacket(this);
        }

        protected override void ToStream(BufferedDataStream bufferedStream)
        {
            bufferedStream.WriteInt32(ChunkGridPositions.Count);
            foreach(Vector2 gridPos in ChunkGridPositions)
            {
                bufferedStream.WriteInt32((int)gridPos.X);
                bufferedStream.WriteInt32((int)gridPos.Y);
            }
        }
    }
}
