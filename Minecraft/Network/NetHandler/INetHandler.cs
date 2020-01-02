﻿namespace Minecraft
{
    interface INetHandler
    {
        void ProcessChatPacket(ChatPacket chatPacker);

        void ProcessPlaceBlockPacket(PlaceBlockPacket blockPacket);

        void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket);

        void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket);

        void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket);

        void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket);

        void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket);

        void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket);

        void ProcessPlayerKickPacket(PlayerKickPacket playerKickPacket);

        void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket);
    }
}
