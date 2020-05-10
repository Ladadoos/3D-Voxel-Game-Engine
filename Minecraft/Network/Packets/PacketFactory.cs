using OpenTK;
using System;
using System.IO;

using System.Collections.Generic;

namespace Minecraft
{
    class PacketFactory
    {
        public Packet ReadPacket(Connection connection)
        {
            BinaryReader reader = connection.Reader;

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
                        int byteSize = reader.ReadInt32();
                        ushort blockId = reader.ReadUInt16();
                        byte[] bytes = reader.ReadBytes(byteSize);
                        BlockState blockState = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();
                        int head = 0;
                        blockState.ExtractFromByteStream(bytes, ref head);
                        return new PlaceBlockPacket(blockState, blockPos);
                    }
                case PacketType.RemoveBlock:
                    {
                        int blockCount = reader.ReadInt32();
                        Vector3i[] blockPositions = new Vector3i[blockCount];
                        for(int i = 0; i < blockCount; i++)
                        {
                            blockPositions[i] = new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());                             
                        }
                        return new RemoveBlockPacket(blockPositions);
                    }
                case PacketType.ChunkData:
                    {
                        int head = 0;
                        Chunk chunk = DataConverter.BytesToChunk(reader.ReadBytes(reader.ReadInt32()), ref head);
                        return new ChunkDataPacket(chunk);
                    }
                case PacketType.ChunkUnload:
                    {
                        int chunkCount = reader.ReadInt32();
                        List<Vector2> chunksToUnload = new List<Vector2>();
                        for(int i = 0; i < chunkCount; i++)
                        {
                            chunksToUnload.Add(new Vector2(reader.ReadInt32(), reader.ReadInt32()));
                        }
                        return new ChunkUnloadPacket(chunksToUnload);
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
                        Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        return new PlayerJoinAcceptPacket(playerName, playerId, position);
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
