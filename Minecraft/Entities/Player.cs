using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class Player
    {       
        private bool isInCreativeMode = false;
        private bool doCollisionDetection = true;

        public Camera camera;
        public Vector3 position;
        public Vector3 velocity;
        public float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

        public AABB hitbox;

        private float verticalSpeed = 0;
        private bool isInAir = true;
        private bool isRunning = false;

        private MouseRay mouseRay;

        private BlockType selectedBlock = BlockType.Dirt;

        public Player(Matrix4 projectionMatrix)
        {
            camera = new Camera();
            position = new Vector3(Constants.CHUNK_SIZE / 2, 148, Constants.CHUNK_SIZE / 2);
            mouseRay = new MouseRay(camera, projectionMatrix);
            hitbox = new AABB(position, GetPlayerMaxAABB());
        }

        public void Update(World world, float deltaTime)
        {
            if (GameWindow.instance.Focused)
            {
                UpdateKeyboardInput(world);
            }

           /* int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int xx = intX - 2; xx <= intX + 2; xx++)
            {
                for (int yy = intY - 2; yy <= intY + 2; yy++)
                {
                    for (int zz = intZ - 2; zz <= intZ + 3; zz++)
                    {
                        BlockType block = world.GetBlockAt(xx, yy, zz);
                        if(block != BlockType.Air)
                        {
                            world.AddBlockToWorld(xx, yy, zz, BlockType.Air);
                        }
                    }
                }
            }*/

            if (!isInCreativeMode)
            {
                UpdateGravityAppliedOnPlayer(deltaTime);
            }

            position.X += velocity.X * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(world);
            }

            position.Y += velocity.Y * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(world);
            }

            position.Z += velocity.Z * deltaTime;
            CalculateHitbox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(world);
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;

            camera.SetPosition(position);
            mouseRay.Update();
            if (Game.input.OnMousePress(MouseButton.Right))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                world.AddBlockToWorld(x, y, z, selectedBlock);
            }
            if (Game.input.OnMousePress(MouseButton.Middle))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                selectedBlock = world.GetBlockAt(x, y, z);
            }
            if (Game.input.OnMousePress(MouseButton.Left))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                world.AddBlockToWorld(x, y, z, BlockType.Air);
            }

            if (GameWindow.instance.Focused)
            {
                camera.Rotate();
                camera.ResetCursor(GameWindow.instance.Bounds);
            }      
        }

        private void UpdateKeyboardInput(World world)
        {
            speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            if ((isInCreativeMode || (!isInCreativeMode && !isInAir)) && 
                (Game.input.OnKeyPress(Key.ControlLeft) || Game.input.OnKeyPress(Key.ControlRight)))
            {
                isRunning = true;
            }

            if (Game.input.OnKeyDown(Key.ShiftLeft) || Game.input.OnKeyDown(Key.ShiftRight))
            {
                if (isInCreativeMode)
                {
                    AddForce(0.0F, -1.0F * speedMultiplier, 0.0F);
                }
                else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            }

            if(isInAir && !isInCreativeMode)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
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
            List<Vector3> collidablePositions = new List<Vector3>();

            int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int xx = intX - 2; xx <= intX + 2; xx++)
            {
                for (int yy = intY - 2; yy <= intY + 2; yy++)
                {
                    for (int zz = intZ - 1; zz <= intZ + Constants.PLAYER_HEIGHT; zz++)
                    {            
                        if(world.GetBlockAt(xx, yy, zz) != BlockType.Air)
                        {
                            collidablePositions.Add(new Vector3(xx, yy, zz));
                        }
                    }
                }
            }

            return collidablePositions;
        }
    }
}
