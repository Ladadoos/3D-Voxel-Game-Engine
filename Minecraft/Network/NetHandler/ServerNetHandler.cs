using OpenTK;
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

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placedBlockpacket)
        {
            game.world.AddBlockToWorld(placedBlockpacket.blockPos, placedBlockpacket.blockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.world.QueueToRemoveBlockAt(removeBlockPacket.blockPos);
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
            game.server.BroadcastPacketExceptTo(playerConnection, playerDataPacket);
            //Logger.Info(playerDataPacket.ToString());
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
            int playerId = game.world.entityIdTracker.GenerateId();
            string serverPlayerName = "server_" + playerName + "_id_" + playerId;
            Player player = new ServerPlayer(playerId, new Vector3(10, 100, 10));

            game.world.playerEntities.Add(playerId, player);
            playerConnection.WritePacket(new PlayerJoinAcceptPacket(serverPlayerName, playerId));
            playerConnection.state = ConnectionState.Accepted;
            game.server.BroadcastPacketExceptTo(playerConnection, new PlayerJoinPacket(serverPlayerName, playerId));
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            throw new InvalidOperationException();
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
            BlockState state = game.world.GetBlockAt(playerInteractionPacket.blockPos);
            //state.GetBlock().OnInteract(state, game.world);
        }
    }
}
