using OpenTK;
using System;

namespace Minecraft
{
    class ServerNetHandler : INetHandler
    {
        private Game game;
        private ServerSession session;

        public ServerNetHandler(Game game)
        {
            this.game = game;
        }

        public void AssignSession(ServerSession session) => this.session = session;

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placedBlockpacket)
        {
            game.server.world.QueueToAddBlockAt(placedBlockpacket.blockPos, placedBlockpacket.blockState);
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

        public void ProcessChunkUnloadPacket(ChunkUnloadPacket unloadChunkPacket)
        {
            throw new InvalidOperationException();
        }

        public void ProcessPlayerDataPacket(PlayerDataPacket playerDataPacket)
        {
            session.player.position = playerDataPacket.position;
            game.server.BroadcastPacketExceptTo(session, playerDataPacket);
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            string playerName = playerJoinRequestPacket.name.Trim();
            if (playerName == string.Empty || playerName == "Player")
            {
                session.WritePacket(new PlayerLeavePacket(0, LeaveReason.Banned, "You are not allowed on this server."));
                session.state = SessionState.Closed;
                return;
            }
            int playerId = game.server.world.GenerateEntityId();
            string serverPlayerName = "server_" + playerName + "_id_" + playerId;
            ServerPlayer player = new ServerPlayer(playerId, serverPlayerName, new Vector3(10, 100, 10));
            session.AssignPlayer(player);

            game.server.world.SpawnEntity(player);
            session.WritePacket(new PlayerJoinAcceptPacket(serverPlayerName, playerId)); // Accept join
            session.state = SessionState.Accepted;

            //Let all the online players know about the new player
            game.server.BroadcastPacketExceptTo(session, new PlayerJoinPacket(serverPlayerName, playerId));

            //let the new player know about all the already only players
            foreach (Session client in game.server.clients)
            {
                if (client.player == player) continue;
                session.WritePacket(new PlayerJoinPacket(client.player.playerName, client.player.id));
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
            Vector3i blockPos = playerInteractionPacket.blockPos;
            BlockState state = game.server.world.GetBlockAt(blockPos);
            state.GetBlock().OnInteract(state, blockPos, game.server.world);

            foreach(ServerSession session in game.server.clients)
            {
                if(session.IsBlockPositionInViewRange(blockPos))
                {
                    session.WritePacket(playerInteractionPacket);
                }
            }
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            game.server.UpdateKeepAliveFor(session);
        }
    }
}
