using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class Player
    {       
        private bool isInCreativeMode = true;
        private bool doCollisionDetection = false;

        public Camera camera;
        public Vector3 position;
        public Vector3 velocity;
        public float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

        public AABB hitbox;

        private float verticalSpeed = 0;
        private bool isInAir = true;
        private bool isRunning = false;

        private MouseRay mouseRay;

        public Player(Matrix4 projectionMatrix)
        {
            camera = new Camera();
            position = new Vector3(Constants.CHUNK_SIZE / 2, 148, Constants.CHUNK_SIZE / 2);
            mouseRay = new MouseRay(camera, projectionMatrix);
            hitbox = new AABB(position, GetPlayerMaxAABB());
        }

        public void Update(World map, float deltaTime)
        {
            if (GameWindow.instance.Focused)
            {
                UpdateKeyboardInput();
            }

            //UpdateWorldGenerationBasedOnPlayerPosition(map);

            if (!isInCreativeMode)
            {
                UpdateGravityAppliedOnPlayer(deltaTime);
            }

            position.X += velocity.X * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(map);
            }

            position.Y += velocity.Y * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(map);
            }

            position.Z += velocity.Z * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(map);
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;

            mouseRay.Update();
            if (Game.input.OnMousePress(MouseButton.Right))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                map.AddBlockToWorld(x, y, z, BlockType.Cobblestone);
            }
            if (Game.input.OnMousePress(MouseButton.Left))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                map.AddBlockToWorld(x, y, z, BlockType.Air);
            }

            camera.SetPosition(position);
            if (GameWindow.instance.Focused)
            {
                camera.Rotate();
                camera.ResetCursor(GameWindow.instance.Bounds);
            }      
        }

        private void UpdateKeyboardInput()
        {
            speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            if ((isInCreativeMode || (!isInCreativeMode && !isInAir)) && 
                (Game.input.OnKeyPress(Key.ControlLeft) || Game.input.OnKeyPress(Key.ControlRight)))
            {
                isRunning = true;
            }

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            }

            if (Game.input.OnKeyDown(Key.W))
            {
                AddForce(0.0F, 0.0F, 1.0F * speedMultiplier);
            }
            if (Game.input.OnKeyDown(Key.S))
            {
                AddForce(0.0F, 0.0F, -1.0F * speedMultiplier);
            }
            if (Game.input.OnKeyDown(Key.D))
            {
                AddForce(1.0F * speedMultiplier, 0.0F, 0.0F);
            }
            if (Game.input.OnKeyDown(Key.A))
            {
                AddForce(-1.0F * speedMultiplier, 0.0F, 0.0F);
            }
            if (Game.input.OnKeyDown(Key.Space))
            {
                if (isInCreativeMode)
                {
                    AddForce(0.0F, 1.0F * speedMultiplier, 0.0F);
                }
                else
                {
                    AttemptToJump();
                }
            }
            if (Game.input.OnKeyDown(Key.ShiftLeft) || Game.input.OnKeyDown(Key.ShiftRight))
            {
                if (isInCreativeMode)
                {
                    AddForce(0.0F, -1.0F * speedMultiplier, 0.0F);
                }
            }

            if (Game.input.OnKeyPress(Key.R))
            {
                int localX = (int)position.X & 15;
                int localY = (int)position.Y & 15;
                int localZ = (int)position.Z & 15;
                Console.WriteLine(localX + "," + localY + "," + localZ);
            }
        }

        private void UpdateGravityAppliedOnPlayer(double deltaTime)
        {
            if (!HasPlayerSurpassedTerminalVelocity())
            {
                verticalSpeed += Constants.GRAVITY * (float)deltaTime;
            }
            else
            {
                verticalSpeed = Constants.GRAVITY_THRESHOLD;
            }

            AddForce(0.0F, verticalSpeed, 0.0F);
        }

        private bool HasPlayerSurpassedTerminalVelocity()
        {
            return verticalSpeed < Constants.GRAVITY_THRESHOLD;
        }

        private void DoXAxisCollisionDetection( World map)
        {
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(map))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.intersects(blockAABB))
                {
                    if (velocity.X > 0.0F)
                    {
                        position.X = blockAABB.min.X - Constants.PLAYER_WIDTH;
                    }
                    if (velocity.X < 0.0F)
                    {
                        position.X = blockAABB.max.X;     
                    }
                    velocity.X = 0.0F;
                    isRunning = false;
                }
            }
        }

        private void DoYAxisCollisionDetection(World map)
        {
            bool collidedY = false;
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(map))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.intersects(blockAABB))
                {
                    if (velocity.Y > 0.0F)
                    {
                        position.Y = blockAABB.min.Y - Constants.PLAYER_HEIGHT;
                    }
                    if (velocity.Y < 0.0F)
                    {
                        position.Y = blockAABB.max.Y;
                        collidedY = true;
                    }
                    velocity.Y = 0.0F;
                    verticalSpeed = 0.0F;
                }
            }

            isInAir = !collidedY;
        }

        private void DoZAxisCollisionDetection( World map)
        {
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(map))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.intersects(blockAABB))
                {
                    if (velocity.Z > 0)
                    {
                        position.Z = blockAABB.min.Z - Constants.PLAYER_LENGTH; 
                    }
                    if (velocity.Z < 0)
                    {
                        position.Z = blockAABB.max.Z;
                    }
                    velocity.Z = 0;
                    isRunning = false;
                }
            }
        }

        private void UpdateWorldGenerationBasedOnPlayerPosition( World map)
        {
            Vector2 chunkPos = map.GetChunkPosition(position.X, position.Z);
            if (!map.chunks.ContainsKey(chunkPos) && Keyboard.GetState().IsKeyDown(Key.Z))
            {
                map.GenerateBlocksForChunk((int)chunkPos.X, (int)chunkPos.Y);
            }

            /*for (int i = -1; i < 3; i++)
            {
                for(int j = -1; j < 3; j++)
                {
                    float x = position.X + i * Constants.CHUNK_SIZE;
                    float z = position.Z + j * Constants.CHUNK_SIZE;
                    Vector2 chunkPos = map.GetChunkPosition(x, z);
                    if (!map.chunks.ContainsKey(chunkPos))
                    {
                        map.GenerateBlocksForChunk((int)chunkPos.X, (int)chunkPos.Y);
                    }
                }
            }*/
        }

        private void AddForce(float x, float y, float z)
        {
            Vector3 offset = new Vector3();
            Vector3 forward = new Vector3((float)Math.Sin(camera.orientation.X), 0, (float)Math.Cos(camera.orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += z * forward;
            offset.Y += y;

            velocity += offset;
        }

        private void AttemptToJump()
        {
            if (!isInAir)
            {
                verticalSpeed = Constants.PLAYER_JUMP_FORCE;
                isInAir = true;
            }
        }

        /// <summary>
        /// Returns the max component of the AABB from this player.
        /// </summary>
        private Vector3 GetPlayerMaxAABB()
        {
            return new Vector3(position.X + Constants.PLAYER_WIDTH, position.Y + Constants.PLAYER_HEIGHT, position.Z + Constants.PLAYER_LENGTH);
        }

        private void CalculateHitbox()
        {
            hitbox.setHitbox(position, GetPlayerMaxAABB());
        }

        private List<Vector3> GetCollisionDetectionBlockPositions( World world)
        {
            //Adapt to player height for collision blocks selection?
            List<Vector3> collidablePositions = new List<Vector3>();

            /*int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int xx = intX - 5; xx <= intX + 5; xx++)
            {
                for (int yy = intY - 5; yy <= intY + 5; yy++)
                {
                    for (int zz = intZ - 5; zz <= intZ + 5; zz++)
                    {
            
                        BlockType block = world.GetBlockAt(intX, intY, intZ);
                        if(block != BlockType.Air)
                        {
                            collidablePositions.Add(new Vector3(xx, yy, zz));
                        }
                    }
                }
            }*/
            Vector2 chunkPos = world.GetChunkPosition(position.X, position.Z);
            sbyte h = (sbyte)(position.Y / Constants.CHUNK_SIZE);
            Chunk chunk;
            Section section;
            world.chunks.TryGetValue(chunkPos, out chunk);
            if (chunk != null)
            {
                section = chunk.sections[h];
                if (section != null)
                {
                    for(int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                if(section.blocks[x, y, z] != null)
                                {
                                    Vector3 v = new Vector3(chunkPos.X * 16 + x, h * 16 + y, chunkPos.Y * 16 + z);
                                    collidablePositions.Add(v);
                                }
                            }
                        }
                    }
                }
            }

             return collidablePositions;
        }
    }
}
