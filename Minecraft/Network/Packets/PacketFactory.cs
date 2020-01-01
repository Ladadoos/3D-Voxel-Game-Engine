using OpenTK;
using System;
using System.IO;

namespace Minecraft
{
    class PacketFactory
    {
        public Packet ReadPacket(Connection connection)
        {
            BinaryReader reader = connection.reader;

            int packetType = reader.ReadInt32();
            PacketType type = (PacketType)packetType;

            switch (type)
            {
                case PacketType.Chat:
                    {
                        string message = ReadUtf8String(reader);
                        return new ChatPacket(message);
                    }
                case PacketType.PlaceBlock:
                    {
                        Vector3i blockPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        int blockId = reader.ReadInt32();
                        BlockState blockState = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                        blockState.FromStream(reader);
                        return new PlaceBlockPacket(blockState, blockPos);
                    }
                case PacketType.RemoveBlock:
                    {
                        Vector3i blockPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        return new RemoveBlockPacket(blockPos);
                    }
                case PacketType.ChunkLoad:
                    {
                        int gridX = reader.ReadInt32();
                        int gridY = reader.ReadInt32();
                        int sectionsNumber = reader.ReadInt32();
                        Chunk chunk = new Chunk(gridX, gridY);
                        for (int i = 0; i < sectionsNumber; i++)
                        {
                            int statesNumber = reader.ReadInt32();
                            for (int j = 0; j < statesNumber; j++)
                            {
                                Vector3i blockPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

                                int blockId = reader.ReadInt32();
                                BlockState blockState = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                                blockState.FromStream(reader);

                                chunk.AddBlock(blockPos.X & 15, blockPos.Y, blockPos.Z & 15, blockState);
                            }
                        }
                        return new ChunkDataPacket(chunk);
                    }
                case PacketType.EntityPosition:
                    {
                        int entityId = reader.ReadInt32();
                        Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        return new PlayerDataPacket(position, entityId);
                    }
                case PacketType.PlayerJoinRequest:
                    {
                        string playerName = ReadUtf8String(reader);
                        return new PlayerJoinRequestPacket(playerName);
                    }
                case PacketType.PlayerJoinAccept:
                    {
                        int playerId = reader.ReadInt32();
                        string playerName = ReadUtf8String(reader);
                        return new PlayerJoinAcceptPacket(playerName, playerId);
                    }
                case PacketType.PlayerKick:
                    {
                        KickReason kickReason = (KickReason)reader.ReadInt32();
                        string message = ReadUtf8String(reader);
                        return new PlayerKickPacket(kickReason, message);
                    }
                case PacketType.PlayerBlockInteraction:
                    {
                        Vector3i blockPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        return new PlayerBlockInteractionPacket(blockPos);
                    }
                default: throw new Exception("Invalid packet type: " + packetType);
            }
        }

        private string ReadUtf8String(BinaryReader reader)
        {
            DataConverter dataConverter = new DataConverter();
            int byteCount = reader.ReadInt32();
            byte[] messageBytes = reader.ReadBytes(byteCount);
            return dataConverter.BytesToUtf8String(messageBytes);
        }
    }
}
