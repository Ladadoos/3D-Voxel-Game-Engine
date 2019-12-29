using System;

namespace Minecraft
{
    class ServerNetHandler : INetHandler
    {
        private Game game;
        private Connection playerConnection;

        public ServerNetHandler(Game game, Connection playerConnection)
        {
            this.game = game;
            this.playerConnection = playerConnection;
        }

        public void ProcessPlaceBlockPacket(PlaceBlockPacket blockPacket)
        {
            game.world.AddBlockToWorld(blockPacket.state.position, blockPacket.state);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.world.QueueToRemoveBlockAt(removeBlockPacket.position);
        }

        public void ProcessChatPacket(ChatPacket chatPacket)
        {
            Logger.Info("Server received message " + chatPacket.message);
            game.localServer.BroadcastPacket(chatPacket);
        }

        public void ProcessChunkDataPacket(ChunkDataPacket chunkDataPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            Logger.Info(playerDataPacket.ToString());
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            string playerName = playerJoinRequestPacket.name;
            playerConnection.SendPacket(new PlayerJoinAcceptPacket("server_" + playerName, 0));
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            throw new InvalidOperationException();
        }
    }
}
