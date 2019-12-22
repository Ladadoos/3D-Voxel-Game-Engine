﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class Player
    {
        private bool isFlying = true;
        private bool isInCreativeMode = true;
        private bool doCollisionDetection = true;
        private bool isInAir = true;
        private bool isCrouching = false;
        private bool isRunning = false;

        private Game game;

        public Camera camera;
        public Vector3 position; //The bottom-left component of the players AABB. 
        public Vector3 velocity;
        public float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

        public AABB hitbox;

        private float verticalSpeed = 0;

        public RayTraceResult mouseOverObject { get; private set; }
        private BlockState selectedBlock = Blocks.Tnt.GetNewDefaultState();

        private Stopwatch jumpStopWatch = new Stopwatch();

        public Player(Game game)
        {
            this.game = game;
            camera = new Camera(game.window, new ProjectionMatrixInfo(0.1F, 1000F, 1.5F, game.window.Width, game.window.Height));
            //position = new Vector3(Constants.CHUNK_SIZE * 8, 148, Constants.CHUNK_SIZE * 8);
            position = new Vector3(0, 120, 0);
            hitbox = new AABB(position, GetPlayerMaxAABB());

            jumpStopWatch.Start();
        }

        public void Update(float deltaTime)
        {
            if (game.window.Focused)
            {
                UpdateKeyboardInput();
            }

            mouseOverObject = new Ray(camera.position, camera.forward).TraceWorld(game.world);

            if (!isFlying)
            {
                UpdateGravityAppliedOnPlayer(deltaTime);
            }

            List<BlockState> blocks = GetCollisionDetectionBlockPositions();
            position.X += velocity.X * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection(blocks);
            }

            position.Y += velocity.Y * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection(blocks);
            }

            position.Z += velocity.Z * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection(blocks);
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;

            Vector3 cameraPosition = position;
            cameraPosition.X += Constants.PLAYER_WIDTH / 2.0F;
            cameraPosition.Y += Constants.PLAYER_CAMERA_HEIGHT;
            cameraPosition.Z += Constants.PLAYER_LENGTH / 2.0F;
            camera.SetPosition(cameraPosition);

            if (Game.input.OnMousePress(MouseButton.Right) && game.window.Focused/* && mouseOverObject != null*/)
            {
                //if (isCrouching)
                //{
                    BlockState newBlock = selectedBlock.GetBlock().GetNewDefaultState();
                    newBlock.position = position + new Vector3(0, 10, 0);
                    game.client.SendPacket(new PlaceBlockPacket(newBlock));

                    //if (selectedBlock.block.CanAddBlockAt(game.world, mouseOverObject.blockPlacePosition))
                    //{
                        /*BlockState newBlock = selectedBlock.block.GetNewDefaultState();
                        newBlock.position = mouseOverObject.blockPlacePosition;
                        game.client.SendPacket(new PlaceBlockPacket(newBlock));*/
                        //game.world.AddBlockToWorld(mouseOverObject.blockPlacePosition, selectedBlock.block.GetNewDefaultState());
                    //}
                /*} else
                {
                    BlockState state = game.world.GetBlockAt(mouseOverObject.blockstateHit.position);
                    if(!state.block.OnInteract(state, game.world))
                    {
                        if (selectedBlock.block.CanAddBlockAt(game.world, mouseOverObject.blockPlacePosition))
                        {
                            //game.world.AddBlockToWorld(mouseOverObject.blockPlacePosition, selectedBlock.block.GetNewDefaultState());
                        }
                    }
                }*/
            }
            if (Game.input.OnMousePress(MouseButton.Middle))
            {
                if (mouseOverObject != null)
                {
                    selectedBlock = game.world.GetBlockAt(mouseOverObject.blockstateHit.position);
                }
            }
            if (Game.input.OnMousePress(MouseButton.Left))
            {
                if (mouseOverObject != null)
                {
                    //game.world.DeleteBlockAt(mouseOverObject.blockstateHit.position);
                }
            }

            if (game.window.Focused)
            {
                camera.Update();
            }
        }

        private void TryStartRunning()
        {
            if (!isRunning && (isInCreativeMode || (!isInCreativeMode && !isInAir)))
            {
                isRunning = true;
                camera.SetFieldOfView(1.65F);
            }
        }

        private void TryToggleFlying()
        {
            jumpStopWatch.Stop();
            float elapsedTime = jumpStopWatch.ElapsedMilliseconds;
            if(elapsedTime < 300 && isInCreativeMode)
            {
                isFlying = !isFlying;
                verticalSpeed = 0;
            }
            jumpStopWatch.Restart();
        }

        private void TryStopRunning()
        {
            if (isRunning)
            {
                isRunning = false;
                camera.SetFieldOfViewToDefault();
            }
        }

        private void TryStopCrouching()
        {
            if (isCrouching)
            {
                isCrouching = false;
                camera.SetFieldOfViewToDefault();
            }
        }

        private void TryStartCrouching()
        {
            if (isRunning)
            {
                TryStopRunning();
            }

            camera.SetFieldOfView(1.45F);
            isCrouching = true;
        }

        private void UpdateKeyboardInput()
        {
            speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            bool inputToRun = Game.input.OnKeyDown(Key.ControlLeft) || Game.input.OnKeyDown(Key.ControlRight);
            bool inputToCrouch = Game.input.OnKeyDown(Key.ShiftLeft) || Game.input.OnKeyDown(Key.ShiftRight);
            bool inputToMoveLeft = Game.input.OnKeyDown(Key.A);
            bool inputToMoveBack = Game.input.OnKeyDown(Key.S);
            bool inputToMoveRight = Game.input.OnKeyDown(Key.D);
            bool inputToMoveForward = Game.input.OnKeyDown(Key.W);
            bool inputToJump = Game.input.OnKeyDown(Key.Space);
            bool inputToFly = Game.input.OnKeyPress(Key.Space);

            //Prioritize crouching over running
            if (inputToRun && !inputToCrouch)
            {
                TryStartRunning();
            } else if (inputToCrouch)
            {
                TryStartCrouching();
            } else if (!inputToCrouch)
            {
                TryStopCrouching();
            }

            if (!inputToMoveForward || inputToMoveBack)
            {
                TryStopRunning();
            }

            if (isInAir && !isFlying)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
            }
            if (isFlying)
            {
                speedMultiplier *= Constants.PLAYER_FLYING_MULTIPLIER;
            }

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            } else if (isCrouching)
            {
                if (isFlying)
                {
                    MovePlayerVertically(-speedMultiplier);
                } else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }

            if (inputToJump)
            {
                if (isFlying)
                {
                    MovePlayerVertically(speedMultiplier);
                } else
                {
                    AttemptToJump();
                }
            }

            if (inputToFly)
            {
                TryToggleFlying();
            }

            if (inputToMoveForward)
            {
                MovePlayerHorizontally(0, speedMultiplier);
            }
            if (inputToMoveBack)
            {
                MovePlayerHorizontally(0, -speedMultiplier);
            }
            if (inputToMoveRight)
            {
                MovePlayerHorizontally(speedMultiplier, 0);
            }
            if (inputToMoveLeft)
            {
                MovePlayerHorizontally(-speedMultiplier, 0);
            }
        }

        private void UpdateGravityAppliedOnPlayer(double deltaTime)
        {
            if (!isInAir)
            {
                return;
            }

            verticalSpeed += (float)(Constants.GRAVITY * deltaTime);
            MovePlayerVertically(verticalSpeed);
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
                foreach(AABB aabb in collidable.GetBlock().GetCollisionBox(collidable))
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

        ///<summary> Moves horizontal relative to the camera. x is right, z is forward. </summary>
        private void MovePlayerHorizontally(float x, float z)
        {
            Vector3 offset = new Vector3();
            Vector3 forward = new Vector3((float)Math.Sin(camera.pitch), 0, (float)Math.Cos(camera.pitch));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);
            offset += x * right;
            offset += z * forward;
            velocity += offset;
        }

        ///<summary> Moves vertical relative to world up vector. </summary>
        private void MovePlayerVertically(float y)
        {
            velocity.Y += y;
        }

        private void AttemptToJump()
        {
            if (!isInAir)
            {
                verticalSpeed = Constants.PLAYER_JUMP_FORCE;
                isInAir = true;
            }
        }

        /// <summary> Returns the max component of the AABB from this player. </summary>
        private Vector3 GetPlayerMaxAABB()
        {
            return new Vector3(position.X + Constants.PLAYER_WIDTH, position.Y + Constants.PLAYER_HEIGHT, position.Z + Constants.PLAYER_LENGTH);
        }

        private void CalculateAABBHitbox()
        {
            hitbox.SetHitbox(position, GetPlayerMaxAABB());
        }

        private List<BlockState> GetCollisionDetectionBlockPositions()
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
                        BlockState blockstate = game.world.GetBlockAt(xx, yy, zz);
                        if (blockstate.GetBlock() != Blocks.Air)
                        {
                            collidables.Add(blockstate);
                        }
                    }
                }
            }

            return collidables;
        }
    }
}
