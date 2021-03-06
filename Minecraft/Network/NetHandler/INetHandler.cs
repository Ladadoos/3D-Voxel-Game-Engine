﻿namespace Minecraft
{
    interface INetHandler
    {
        void ProcessChatPacket(ChatPacket chatPacker);

        void ProcessPlaceBlockPacket(PlaceBlockPacket blockPacket);

        void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket);

        void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket);

        void ProcessChunkUnloadPacket(ChunkUnloadPacket unloadChunkPacket);

        void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket);

        void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket);

        void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket);

        void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket);

        void ProcessPlayerLeavePacket(PlayerLeavePacket playerKickPacket);

        void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket);

        void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket);
    }
}
