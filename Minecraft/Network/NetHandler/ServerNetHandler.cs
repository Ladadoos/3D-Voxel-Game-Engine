using OpenTK;
using System;

namespace Minecraft
{
    class ServerNetHandler : INetHandler
    {
        private readonly Game game;
        private ServerSession session;

        public ServerNetHandler(Game game)
        {
            this.game = game;
        }

        public void AssignSession(ServerSession session) => this.session = session;

        public void ProcessPlaceBlockPacket(PlaceBlockPacket placeBlockpacket)
        {
            game.Server.World.QueueToAddBlockAt(placeBlockpacket.BlockPos, placeBlockpacket.BlockState);
        }

        public void ProcessRemoveBlockPacket(RemoveBlockPacket removeBlockPacket)
        {
            foreach(Vector3i blockPos in removeBlockPacket.BlockPositions)
            {
                game.Server.World.QueueToRemoveBlockAt(blockPos);
            }
        }

        public void ProcessChatPacket(ChatPacket chatPacket)
        {
            Logger.Info("Server received message " + chatPacket.Message);
            game.Server.BroadcastPacket(chatPacket);
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
            session.Player.Position = playerDataPacket.Position;
            session.Player.Velocity = playerDataPacket.Velocity;
            game.Server.BroadcastPacketExceptTo(session, playerDataPacket);
        }

        public void ProcessJoinRequestPacket(PlayerJoinRequestPacket playerJoinRequestPacket)
        {
            string playerName = playerJoinRequestPacket.Name.Trim();
            if (playerName == string.Empty || playerName == "Player")
            {
                session.WritePacket(new PlayerLeavePacket(0, LeaveReason.Banned, "You are not allowed on this server."));
                session.State = SessionState.Closed;
                return;
            }

            int playerId = game.Server.World.GenerateEntityId();
            string serverPlayerName = playerName + "-" + playerId % 10000;
            Vector3 spawnPosition = game.Server.World.GenerateAndGetValidSpawn();

            ServerPlayer player = new ServerPlayer(playerId, serverPlayerName, game.Server.World, spawnPosition);
            session.AssignPlayer(player);

            game.Server.World.SpawnEntity(player);
            session.WritePacket(new PlayerJoinAcceptPacket(serverPlayerName, playerId, spawnPosition, game.Server.World.Environment.CurrentTime)); // Accept join
            session.State = SessionState.Accepted;

            //Let all the online players know about the new player
            game.Server.BroadcastPacketExceptTo(session, new PlayerJoinPacket(serverPlayerName, playerId));

            //let the new player know about all the already only players
            foreach (Session client in game.Server.ConnectedClients)
            {
                if (client.Player == player) continue;
                session.WritePacket(new PlayerJoinPacket(client.Player.Name, client.Player.ID));
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
            Vector3i blockPos = playerInteractionPacket.BlockPos;
            BlockState state = game.Server.World.GetBlockAt(blockPos);
            state.GetBlock().OnInteract(state, blockPos, game.Server.World);

            foreach(ServerSession clientSession in game.Server.ConnectedClients)
            {
                if(clientSession.IsBlockPositionInViewRange(blockPos))
                {
                    clientSession.WritePacket(playerInteractionPacket);
                }
            }
        }

        public void ProcessPlayerKeepAlivePacket(PlayerKeepAlivePacket keepAlivePacket)
        {
            game.Server.UpdateKeepAliveFor(session);
        }
    }
}
