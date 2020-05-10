using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minecraft
{
    abstract class Player : Entity
    {
        public string Name { get; set; }

        protected bool isFlying = true;
        protected bool isInCreativeMode = true;
        protected bool doCollisionDetection = true;
        protected bool isInAir = true;
        protected bool isCrouching = false;
        protected bool isRunning = false;
        protected Stopwatch jumpStopWatch = new Stopwatch();

        protected Vector3 realForward; //Vector facing towards where the player is looking
        protected Vector3 moveForward; //Vector facing where the player is looking, ignoring y
        protected Vector3 right;       //Vector facing to the right of where the player is looking
        protected float verticalSpeed;

        protected delegate void OnToggleRunning(bool isRunning);
        protected event OnToggleRunning OnToggleRunningHandler;

        protected delegate void OnToggleCrouching(bool isCrouching);
        protected event OnToggleCrouching OnToggleCrouchingHandler;

        protected Player(int id, string playerName, Vector3 startPosition) : base(id, startPosition, EntityType.Player)
        {
            Name = playerName;
            jumpStopWatch.Start();
        }

        protected override void SetInitialDimensions()
        {
            width = Constants.PLAYER_WIDTH;
            height = Constants.PLAYER_HEIGHT;
            length = Constants.PLAYER_LENGTH;
        }

        ///<summary> Moves horizontal relative to the direction the player is facing. x is right, z is forward. </summary>
        protected void MovePlayerHorizontally(float x, float z)
        {
            Vector3 offset = new Vector3();
            offset += x * right;
            offset += z * moveForward;
            Acceleration += offset;
        }

        ///<summary> Moves vertical relative to world up vector. </summary>
        protected void MovePlayerVertically(float y)
        {
            Acceleration.Y += y;
        }

        protected void TryApplyGravity(double deltaTime)
        {
            if (!isInAir)
            {
                return;
            }

            verticalSpeed += (float)(Constants.GRAVITY * deltaTime);
            MovePlayerVertically(verticalSpeed);
        }

        protected void TryStartRunning()
        {
            if (!isRunning && (isInCreativeMode || (!isInCreativeMode && !isInAir)))
            {
                isRunning = true;
                OnToggleRunningHandler?.Invoke(isRunning);
            }
        }

        protected void TryStopRunning()
        {
            if (isRunning)
            {
                isRunning = false;
                OnToggleRunningHandler?.Invoke(isRunning);
            }
        }

        protected void TryToggleFlying()
        {
            jumpStopWatch.Stop();
            float elapsedTime = jumpStopWatch.ElapsedMilliseconds;
            if (elapsedTime < 300 && isInCreativeMode)
            {
                isFlying = !isFlying;
                verticalSpeed = 0;
            }
            jumpStopWatch.Restart();
        }

        protected void TryStopCrouching()
        {
            if (isCrouching)
            {
                isCrouching = false;
                OnToggleCrouchingHandler?.Invoke(isCrouching);
            }
        }

        protected void TryStartCrouching()
        {
            if (isRunning)
            {
                TryStopRunning();
            }

            isCrouching = true;
            OnToggleCrouchingHandler?.Invoke(isCrouching);
        }

        protected void AttemptToJump()
        {
            if (!isInAir)
            {
                verticalSpeed = Constants.PLAYER_JUMP_FORCE;
                isInAir = true;
            }
        }

        protected void ApplyVelocityAndCheckCollision(float deltaTime, World world)
        {
            if (!isFlying)
            {
                TryApplyGravity(deltaTime);
            }

            Dictionary<Vector3i, BlockState> blocks = new Dictionary<Vector3i, BlockState>();
            if (doCollisionDetection)
            {
                blocks = GetCollisionDetectionBlocks(world);
            }

            Velocity.X += Acceleration.X * deltaTime;
            Position.X += Velocity.X * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(blocks);
            }

            Velocity.Y += Acceleration.Y * deltaTime;
            Position.Y += Velocity.Y * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(blocks);
            }

            Velocity.Z += Acceleration.Z * deltaTime;
            Position.Z += Velocity.Z * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(blocks);
            }

            float friction = -10F;
            Velocity += friction * Velocity * deltaTime;
        }

        /// <summary> Returns all blocks around the players position used for collision detection </summary>
        private Dictionary<Vector3i, BlockState> GetCollisionDetectionBlocks(World world)
        {
            Dictionary<Vector3i, BlockState> collidables = new Dictionary<Vector3i, BlockState>();

            Vector3i pos = new Vector3i(Position);
            for (int worldX = pos.X - 1; worldX <= pos.X + 1; worldX++)
            {
                for (int worldY = pos.Y - 1; worldY <= pos.Y + Math.Ceiling(Constants.PLAYER_HEIGHT); worldY++)
                {
                    for (int worldZ = pos.Z - 1; worldZ <= pos.Z + 1; worldZ++)
                    {
                        Vector3i blockPos = new Vector3i(worldX, worldY, worldZ);
                        BlockState blockstate = world.GetBlockAt(blockPos);
                        if (blockstate.GetBlock() != Blocks.Air)
                        {
                            collidables.Add(blockPos, blockstate);
                        }
                    }
                }
            }

            return collidables;
        }

        private void DoXAxisCollisionDetection(Dictionary<Vector3i, BlockState> blocks)
        {
            foreach (KeyValuePair<Vector3i, BlockState> collidable in blocks)
            {
                foreach (AxisAlignedBox aabb in collidable.Value.GetBlock().GetCollisionBox(collidable.Value, collidable.Key))
                {
                    if (!Hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (Velocity.X > 0.0F)
                    {
                        Position.X = aabb.Min.X - Constants.PLAYER_WIDTH;
                    }
                    if (Velocity.X < 0.0F)
                    {
                        Position.X = aabb.Max.X;
                    }
                    Velocity.X = 0.0F;
                    TryStopRunning();
                }
            }
        }

        private void DoYAxisCollisionDetection(Dictionary<Vector3i, BlockState> blocks)
        {
            bool collidedY = false;
            foreach (KeyValuePair<Vector3i, BlockState> collidable in blocks)
            {
                foreach (AxisAlignedBox aabb in collidable.Value.GetBlock().GetCollisionBox(collidable.Value, collidable.Key))
                {
                    if (!Hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (Velocity.Y > 0.0F)
                    {
                        Position.Y = aabb.Min.Y - Constants.PLAYER_HEIGHT;
                    }
                    if (Velocity.Y < 0.0F)
                    {
                        Position.Y = aabb.Max.Y;
                        collidedY = true;
                    }
                    Velocity.Y = 0.0F;
                    verticalSpeed = 0.0F;
                }
            }

            isInAir = !collidedY;
        }

        private void DoZAxisCollisionDetection(Dictionary<Vector3i, BlockState> blocks)
        {
            foreach (KeyValuePair<Vector3i, BlockState> collidable in blocks)
            {
                foreach (AxisAlignedBox aabb in collidable.Value.GetBlock().GetCollisionBox(collidable.Value, collidable.Key))
                {
                    if (!Hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (Velocity.Z > 0)
                    {
                        Position.Z = aabb.Min.Z - Constants.PLAYER_LENGTH;
                    }
                    if (Velocity.Z < 0)
                    {
                        Position.Z = aabb.Max.Z;
                    }
                    Velocity.Z = 0;
                    TryStopRunning();
                }
            }
        }
    }
}
