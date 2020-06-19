using OpenTK;
using System;
using System.IO;

using System.Collections.Generic;

namespace Minecraft
{
    class PacketFactory
    {
        public Packet ReadPacket(Session session)
        {
            BinaryReader reader = session.Connection.Reader;

            int packetType = reader.ReadInt32();
            PacketType type = (PacketType)packetType;

            switch (type)
            {
                case PacketType.Chat:
                    {
                        string sender = ReadUtf8String(reader);
                        string message = ReadUtf8String(reader);
                        return new ChatPacket(sender, message);
                    }
                case PacketType.PlaceBlock:
                    {
                        Vector3i blockPos = ReadVector3i(reader);
                        int byteSize = reader.ReadInt32();
                        ushort blockId = reader.ReadUInt16();
                        byte[] bytes = reader.ReadBytes(byteSize);
                        BlockState blockState = Blocks.GetState(Blocks.GetBlockFromIdentifier(blockId));
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
                            blockPositions[i] = ReadVector3i(reader);                             
                        }
                        return new RemoveBlockPacket(blockPositions);
                    }
                case PacketType.ChunkData:
                    {
                        int head = 0;
                        int chunkByteSize = reader.ReadInt32();
                        byte[] chunkBytes = reader.ReadBytes(chunkByteSize);
                        Chunk chunk = DataConverter.BytesToChunk(chunkBytes, session.Player.World, ref head);
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
                        Vector3 position = ReadVector3(reader);
                        Vector3 velocity = ReadVector3(reader);
                        return new PlayerDataPacket(entityId, position, velocity);
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
                        Vector3 position = ReadVector3(reader);
                        float currentTime = reader.ReadSingle();
                        return new PlayerJoinAcceptPacket(playerName, playerId, position, currentTime);
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
                        Vector3i blockPos = ReadVector3i(reader);
                        return new PlayerBlockInteractionPacket(blockPos);
                    }
                case PacketType.PlayerKeepAlive:
                    {
                        return new PlayerKeepAlivePacket();
                    }
                default: throw new Exception("Invalid packet type: " + packetType);
            }
        }

        private Vector3 ReadVector3(BinaryReader reader) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        private Vector3i ReadVector3i(BinaryReader reader) => new Vector3i(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        private string ReadUtf8String(BinaryReader reader)
        {    
            int byteCount = reader.ReadInt32();
            byte[] messageBytes = reader.ReadBytes(byteCount);
            return DataConverter.BytesToUtf8String(messageBytes);
        }
    }
}
