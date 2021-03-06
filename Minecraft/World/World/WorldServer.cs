﻿using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    /// <summary>
    /// The representation of the world used on the server.
    /// </summary>
    class WorldServer : World
    {
        /// <summary>
        /// All chunks inside a square with side (radius * 2) + 1 at the world origin will not unload.
        /// </summary>
        private const int spawnAreaRadius = 3;

        private IdTracker entityIdTracker = new IdTracker();
        private WorldGenerator worldGenerator;

        public WorldServer(Game game) : base(game)
        {
            OnBlockPlacedHandler += OnBlockPlacedServer;
            OnBlockRemovedHandler += OnBlockRemovedServer;
            OnEntityDespawnedHandler += OnEntityDespawnedServer;

            worldGenerator = new WorldGenerator(this);
            LoadSpawnArea();
        }

        private void LoadSpawnArea()
        {
            for(int x = -spawnAreaRadius; x <= spawnAreaRadius; x++)
            {
                for(int y = -spawnAreaRadius; y <= spawnAreaRadius; y++)
                {
                    AddPlayerPresenceToChunk(worldGenerator.GenerateBlocksForChunkAt(x, y));
                }
            }                             
        }

        /// <summary>
        /// Creates a suitable spawn position. Destroys blocks if necessary.
        /// </summary>
        public Vector3 GenerateAndGetValidSpawn()
        {
            bool foundSpawn = false;
            Vector3 spawnPosition = Vector3.Zero;

            //Check if there is a suitable position in the middle of the chunk at the origin of the world.
            const int x = 8;
            const int z = 8;
            for(int y = worldGenerator.SeaLevel; y < Constants.MAX_BUILD_HEIGHT - 3; y++)
            {
                int offset = 0;
                while(GetBlockAt(new Vector3i(x, y + offset, z)).GetBlock() == Blocks.Air && offset < 3)
                {
                    offset++;
                }

                if(offset == 3)
                {
                    foundSpawn = true;
                    spawnPosition = new Vector3(x, y, z);
                    break;
                }                   
            }

            if(foundSpawn)
            {
                return spawnPosition;
            }

            //Create a platform to spawn on
            const int platformSize = 3;
            for(int xOffset = -platformSize; xOffset < platformSize; xOffset++)
            {
                for(int zOffset = -platformSize; zOffset < platformSize; zOffset++)
                {
                    QueueToAddBlockAt(new Vector3i(x + xOffset, worldGenerator.SeaLevel, z + zOffset), Blocks.GetState(Blocks.Stone));
                }
            }

            //Remove the blocks ontop of the platform
            List<Vector3i> toRemoveBlocks = new List<Vector3i>();
            for(int xOffset = -platformSize; xOffset < platformSize; xOffset++)
            {
                for(int zOffset = -platformSize; zOffset < platformSize; zOffset++)
                {
                    for(int yOffset = 1; yOffset <= platformSize; yOffset++)
                    {
                        toRemoveBlocks.Add(new Vector3i(x + xOffset, worldGenerator.SeaLevel + yOffset, z + zOffset));
                    }                        
                }
            }

            QueueToRemoveBlocksAt(toRemoveBlocks);
            ClearBlockRemoveBuffer();
            ClearBlockAddBuffer();

            return new Vector3(x, worldGenerator.SeaLevel, z);
        }

        public int GenerateEntityId() => entityIdTracker.GenerateId();

        public void RequestGenerationOfChunk(int playerId, Vector2 gridPosition, Action<GenerateChunkOutput> callback)
        {
            worldGenerator.AddChunkGenerationRequest(new GenerateChunkRequest()
            {
                playerId = playerId,
                gridPosition = gridPosition,
                world = this,
                callback = callback
            });
        }

        private void OnEntityDespawnedServer(World world, Entity entity)
        {
            entityIdTracker.ReleaseId(entity.ID);

            if(entity is ServerPlayer)
            {
                Game.Server.BroadcastPacket(new PlayerLeavePacket(entity.ID, LeaveReason.Leave, "disconnect"));
            }
        }

        private void OnBlockPlacedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState, BlockState newState)
        {
            foreach(ServerSession session in Game.Server.ConnectedClients)
            {
                if(session.IsBlockPositionInViewRange(blockPos))
                {
                    session.WritePacket(new PlaceBlockPacket(newState, blockPos));
                }
            }           
        }

        private void OnBlockRemovedServer(World world, Chunk chunk, Vector3i blockPos, BlockState oldState)
        {
            RemoveBlockPacket packet = new RemoveBlockPacket(new Vector3i[] { blockPos });
            foreach(ServerSession session in Game.Server.ConnectedClients)
            {
                if(session.IsBlockPositionInViewRange(blockPos))
                {
                    session.WritePacket(packet);
                }
            }
        }
    }
}
