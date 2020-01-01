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
            game.server.BroadcastPacket(chatPacket);
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
            string playerName = playerJoinRequestPacket.name.Trim();
            if(playerName == string.Empty || playerName == "Player")
            {
                playerConnection.WritePacket(new PlayerKickPacket(KickReason.Banned, "You are not allowed on this server."));
                playerConnection.state = ConnectionState.Closed;
                return;
            }
            playerConnection.WritePacket(new PlayerJoinAcceptPacket("server_" + playerName, 0));
            playerConnection.state = ConnectionState.Accepted;
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessPlayerKickPacket(PlayerKickPacket playerKickPacket)
        {
            throw new NotImplementedException();
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            BlockState state = game.world.GetBlockAt(playerInteractionPacket.intPosition);
            //state.GetBlock().OnInteract(state, game.world);
        }
    }
}
