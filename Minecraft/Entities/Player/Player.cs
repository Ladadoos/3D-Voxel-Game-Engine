using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minecraft
{
    abstract class Player : Entity
    {
        public string playerName;

        protected bool isFlying = true;
        protected bool isInCreativeMode = true;
        protected bool doCollisionDetection = false;
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

        public Player(int id, string playerName, Vector3 startPosition) : base(id, startPosition, EntityType.Player)
        {
            this.playerName = playerName;
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
            velocity += offset;
        }

        ///<summary> Moves vertical relative to world up vector. </summary>
        protected void MovePlayerVertically(float y)
        {
            velocity.Y += y;
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
                
            position.X += velocity.X * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(blocks);
            }

            position.Y += velocity.Y * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(blocks);
            }

            position.Z += velocity.Z * deltaTime;
            UpdateAxisAlignedBox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(blocks);
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;
        }

        /// <summary> Returns all blocks around the players position used for collision detection </summary>
        private Dictionary<Vector3i, BlockState> GetCollisionDetectionBlocks(World world)
        {
            Dictionary<Vector3i, BlockState> collidables = new Dictionary<Vector3i, BlockState>();

            int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int worldX = intX - 1; worldX <= intX + 1; worldX++)
            {
                for (int worldY = intY - 1; worldY <= intY + Math.Ceiling(Constants.PLAYER_HEIGHT); worldY++)
                {
                    for (int worldZ = intZ - 1; worldZ <= intZ + 1; worldZ++)
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
                    if (!hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (velocity.X > 0.0F)
                    {
                        position.X = aabb.min.X - Constants.PLAYER_WIDTH;
                    }
                    if (velocity.X < 0.0F)
                    {
                        position.X = aabb.max.X;
                    }
                    velocity.X = 0.0F;
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
                    if (!hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (velocity.Y > 0.0F)
                    {
                        position.Y = aabb.min.Y - Constants.PLAYER_HEIGHT;
                    }
                    if (velocity.Y < 0.0F)
                    {
                        position.Y = aabb.max.Y;
                        collidedY = true;
                    }
                    velocity.Y = 0.0F;
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
                    if (!hitbox.Intersects(aabb))
                    {
                        continue;
                    }

                    if (velocity.Z > 0)
                    {
                        position.Z = aabb.min.Z - Constants.PLAYER_LENGTH;
                    }
                    if (velocity.Z < 0)
                    {
                        position.Z = aabb.max.Z;
                    }
                    velocity.Z = 0;
                    TryStopRunning();
                }
            }
        }
    }
}
