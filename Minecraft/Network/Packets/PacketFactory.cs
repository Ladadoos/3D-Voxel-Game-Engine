using OpenTK;
using System;
using System.IO;

namespace Minecraft
{
    class PacketFactory
    {
        public Packet ReadPacket(BinaryReader reader)
        {
            PacketType type = (PacketType)reader.ReadInt32();

            switch (type)
            {
                case PacketType.Chat:
                    {
                        int byteCount = reader.ReadInt32();
                        byte[] messageBytes = reader.ReadBytes(byteCount);
                        DataConverter converter = new DataConverter();
                        string message = converter.BytesToUtf8String(messageBytes);
                        return new ChatPacket(message);
                    }
                case PacketType.BlockPlaced:
                    {
                        int blockId = reader.ReadInt32();
                        Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        BlockState state = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                        state.position = position;
                        return new PlaceBlockPacket(state);
                    }
                case PacketType.ChunkLoaded:
                    {
                        int gridX = reader.ReadInt32();
                        int gridY = reader.ReadInt32();
                        int sectionsNumber = reader.ReadInt32();
                        Chunk chunk = new Chunk(gridX, gridY);
                        for(int i = 0; i < sectionsNumber; i++)
                        {
                            int statesNumber = reader.ReadInt32();
                            for(int j = 0; j < statesNumber; j++)
                            {
                                int blockId = reader.ReadInt32();
                                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                                BlockState state = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                                state.position = position;

                                chunk.AddBlock((int)position.X & 15, (int)position.Y, (int)position.Z & 15, state);
                            }
                        }
                        return new ChunkDataPacket(chunk);
                    }
                default: throw new Exception("Invalid packet type.");
            }
        }
    }
}
