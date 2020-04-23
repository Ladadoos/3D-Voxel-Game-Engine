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
                        ushort blockId = reader.ReadUInt16();
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
                        int gridX = reader.ReadInt32();
                        int gridY = reader.ReadInt32();
                        int totalSize = reader.ReadInt32();

                        Chunk chunk = new Chunk(gridX, gridY);

                        byte[] chunkBytes = reader.ReadBytes(totalSize);
                        
                        int pos = 0;
                        for(int i = 0; i < 16; i++)
                        {
                            Section section = new Section(gridX, gridY, (byte)i);
                            byte isSectionEmpty = chunkBytes[pos];
                            pos++;
                            if(isSectionEmpty != 0)
                            {
                                for(int x = 0; x < 16; x++)
                                {
                                    for(int y = 0; y < 16; y++)
                                    {
                                        for(int z = 0; z < 16; z++)
                                        {        
                                            ushort blockId = DataConverter.BytesToUInt16(chunkBytes, pos);
                                            pos += 2;
                                            if(blockId != 0)
                                            {
                                                BlockState blockState = Blocks.GetBlockFromIdentifier(blockId).GetNewDefaultState();        
                                                blockState.ExtractFromByteStream(chunkBytes, pos);
                                                pos += blockState.PayloadSize();
                                                section.AddBlock(x, y, z, blockState);
                                            }
                                        }
                                    }
                                }
                            }

                            chunk.sections[i] = section;
                        }

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
