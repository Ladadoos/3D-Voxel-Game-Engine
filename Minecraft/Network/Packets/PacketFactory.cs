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
                case PacketType.ChunkData:
                    {
                        int byteSize = reader.ReadInt32();
                        byte[] bytes = reader.ReadBytes(byteSize);
                        Chunk chunk = (Chunk)DataConverter.ByteArrayToObject(bytes);
                        return new ChunkDataPacket(chunk);
                    }
                case PacketType.ChunkDataRequest:
                    {
                        int gridX = reader.ReadInt32();
                        int gridZ = reader.ReadInt32();
                        return new ChunkDataRequestPacket(gridX, gridZ);
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
                case PacketType.PlayerJoin:
                    {
                        int playerId = reader.ReadInt32();
                        string playerName = ReadUtf8String(reader);
                        return new PlayerJoinPacket(playerName, playerId);
                    }
                case PacketType.PlayerLeave:
                    {
                        int id = reader.ReadInt32();
                        LeaveReason kickReason = (LeaveReason)reader.ReadInt32();
                        string message = ReadUtf8String(reader);
                        return new PlayerLeavePacket(id, kickReason, message);
                    }
                case PacketType.PlayerBlockInteraction:
                    {
                        Vector3i blockPos = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                        return new PlayerBlockInteractionPacket(blockPos);
                    }
                case PacketType.PlayerKeepAlive:
                    {
                        return new PlayerKeepAlivePacket();
                    }
                default: throw new Exception("Invalid packet type: " + packetType);
            }
        }

        private string ReadUtf8String(BinaryReader reader)
        {    
            int byteCount = reader.ReadInt32();
            byte[] messageBytes = reader.ReadBytes(byteCount);
            return DataConverter.BytesToUtf8String(messageBytes);
        }
    }
}
