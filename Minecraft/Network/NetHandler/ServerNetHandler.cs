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
            game.server.world.AddBlockToWorld(placedBlockpacket.blockPos, placedBlockpacket.blockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            game.server.world.QueueToRemoveBlockAt(removeBlockPacket.blockPos);
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
            playerConnection.player.position = playerDataPacket.position;
            game.server.BroadcastPacketExceptTo(playerConnection, playerDataPacket);
            //Logger.Info(playerDataPacket.ToString());
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            string playerName = playerJoinRequestPacket.name.Trim();
            if (playerName == string.Empty || playerName == "Player")
            {
                playerConnection.WritePacket(new PlayerLeavePacket(0, LeaveReason.Banned, "You are not allowed on this server."));
                playerConnection.state = ConnectionState.Closed;
                return;
            }
            int playerId = game.server.world.entityIdTracker.GenerateId();
            string serverPlayerName = "server_" + playerName + "_id_" + playerId;
            ServerPlayer player = new ServerPlayer(playerId, new Vector3(10, 100, 10));
            playerConnection.player = player;

            game.server.world.loadedEntities.Add(playerId, player);
            playerConnection.WritePacket(new PlayerJoinAcceptPacket(serverPlayerName, playerId));
            playerConnection.state = ConnectionState.Accepted;
            game.server.BroadcastPacketExceptTo(playerConnection, new PlayerJoinPacket(serverPlayerName, playerId));

            foreach (Connection client in game.server.clients)
            {
                if (client.player == player)
                {
                    continue;
                }
                playerConnection.WritePacket(new PlayerJoinPacket("", client.player.id));
            }
        }

        public void ProcessPlayerJoinPacket(PlayerJoinPacket playerJoinPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessJoinAcceptPacket(PlayerJoinAcceptPacket playerJoinAcceptPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessPlayerLeavePacket(PlayerLeavePacket playerKickPacket)
        {
            throw new NotImplementedException();
        }

        public void ProcessPlayerBlockInteractionpacket(PlayerBlockInteractionPacket playerInteractionPacket)
        {
            BlockState state = game.server.world.GetBlockAt(playerInteractionPacket.blockPos);
            //state.GetBlock().OnInteract(state, game.world);
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            game.server.UpdateKeepAliveFor(playerConnection);
        }
    }
}
