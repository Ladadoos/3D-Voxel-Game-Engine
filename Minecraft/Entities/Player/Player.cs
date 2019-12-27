using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Minecraft
{
    abstract class Player
    {
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
        protected Vector3 position;    //Bottom-left of player AABB
        protected Vector3 velocity;
        protected float verticalSpeed;
        protected AABB hitbox;

        protected delegate void OnToggleRunning(bool isRunning);
        protected event OnToggleRunning OnToggleRunningHandler;

        protected delegate void OnToggleCrouching(bool isCrouching);
        protected event OnToggleCrouching OnToggleCrouchingHandler;

        public Player(Vector3 startPosition)
        {
            position = startPosition;
            velocity = Vector3.Zero;
            hitbox = new AABB(position, GetPlayerMaxAABB());
            jumpStopWatch.Start();
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public Vector3 GetVelocity()
        {
            return velocity;
        }

        public abstract void Update(float deltaTime, World world);

        /// <summary> Returns the max component of the AABB from this player. </summary>
        protected Vector3 GetPlayerMaxAABB()
        {
            return new Vector3(position.X + Constants.PLAYER_WIDTH, position.Y + Constants.PLAYER_HEIGHT, position.Z + Constants.PLAYER_LENGTH);
        }

        protected void CalculatePlayerAABB()
        {
            hitbox.SetHitbox(position, GetPlayerMaxAABB());
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

            List<BlockState> blocks = GetCollisionDetectionBlocks(world);
            position.X += velocity.X * deltaTime;
            CalculatePlayerAABB();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(blocks);
            }

            position.Y += velocity.Y * deltaTime;
            CalculatePlayerAABB();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(blocks);
            }

            position.Z += velocity.Z * deltaTime;
            CalculatePlayerAABB();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(blocks);
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;
        }

        /// <summary> Returns all blocks around the players position used for collision detection </summary>
        private List<BlockState> GetCollisionDetectionBlocks(World world)
        {
            List<BlockState> collidables = new List<BlockState>();

            int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int xx = intX - 1; xx <= intX + 1; xx++)
            {
                for (int yy = intY - 1; yy <= intY + Math.Ceiling(Constants.PLAYER_HEIGHT); yy++)
                {
                    for (int zz = intZ - 1; zz <= intZ + 1; zz++)
                    {
                        BlockState blockstate = world.GetBlockAt(xx, yy, zz);
                        if (blockstate.GetBlock() != Blocks.Air)
                        {
                            collidables.Add(blockstate);
                        }
                    }
                }
            }

            return collidables;
        }

        private void DoXAxisCollisionDetection(List<BlockState> blocks)
        {
            foreach (BlockState collidable in blocks)
            {
                foreach (AABB aabb in collidable.GetBlock().GetCollisionBox(collidable))
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

        private void DoYAxisCollisionDetection(List<BlockState> blocks)
        {
            bool collidedY = false;
            foreach (BlockState collidable in blocks)
            {
                foreach (AABB aabb in collidable.GetBlock().GetCollisionBox(collidable))
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

        private void DoZAxisCollisionDetection(List<BlockState> blocks)
        {
            foreach (BlockState collidable in blocks)
            {
                foreach (AABB aabb in collidable.GetBlock().GetCollisionBox(collidable))
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
