using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class Player
    {
        private bool isInCreativeMode = true;
        private bool doCollisionDetection = true;
        private bool isInAir = true;
        private bool isCrouching = false;
        private bool isRunning = false;

        private Game game;

        public Camera camera;
        public Vector3 position;
        public Vector3 velocity;
        public float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

        public AABB hitbox;

        private float verticalSpeed = 0;

        private BlockType selectedBlock = BlockType.Cobblestone;

        public Player(Game game)
        {
            this.game = game;
            camera = new Camera(new ProjectionMatrixInfo(0.1F, 1000F, 1.5F, game.window.Width, game.window.Height));
            position = new Vector3(Constants.CHUNK_SIZE * 8, 148, Constants.CHUNK_SIZE * 8);
            hitbox = new AABB(position, GetPlayerMaxAABB());
        }

        public void Update(float deltaTime)
        {
            if (game.window.Focused)
            {
                UpdateKeyboardInput();
            }

            /*int intX = (int)position.X;
            int intY = (int)position.Y;
            int intZ = (int)position.Z;

            for (int xx = intX - 2; xx <= intX + 2; xx++)
            {
                for (int yy = intY - 2; yy <= intY + 2; yy++)
                {
                    for (int zz = intZ - 2; zz <= intZ + 3; zz++)
                    { 
                        if(game.world.GetBlockAt(xx, yy, zz) != BlockType.Air)
                        {
                            game.world.AddBlockToWorld(xx, yy, zz, BlockType.Air);
                        }
                    }
                }
            }*/

            if (!isInCreativeMode)
            {
                UpdateGravityAppliedOnPlayer(deltaTime);
            }

            position.X += velocity.X * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoXAxisCollisionDetection();
            }

            position.Y += velocity.Y * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoYAxisCollisionDetection();
            }

            position.Z += velocity.Z * deltaTime;
            CalculateAABBHitbox();
            if (doCollisionDetection)
            {
                DoZAxisCollisionDetection();
            }

            velocity *= Constants.PLAYER_STOP_FORCE_MULTIPLIER;

            Vector3 cameraPosition = position;
            cameraPosition.X += Constants.PLAYER_WIDTH / 2.0F;
            cameraPosition.Y += Constants.PLAYER_CAMERA_HEIGHT;
            cameraPosition.Z += Constants.PLAYER_LENGTH / 2.0F;
            camera.SetPosition(cameraPosition);

            if (Game.input.OnMousePress(MouseButton.Right))
            {
                int offset = 2;
                int x = (int)Math.Floor(camera.position.X + camera.forward.X * offset);
                int y = (int)Math.Floor(camera.position.Y + camera.forward.Y * offset);
                int z = (int)Math.Floor(camera.position.Z + camera.forward.Z * offset);

                game.world.AddBlockToWorld(x, y, z, selectedBlock);
            }
            if (Game.input.OnMousePress(MouseButton.Middle))
            {
                int offset = 2;
                int x = (int)(camera.position.X + camera.forward.X * offset);
                int y = (int)(camera.position.Y + camera.forward.Y * offset);
                int z = (int)(camera.position.Z + camera.forward.Z * offset);

                selectedBlock = game.world.GetBlockAt(x, y, z);
            }
            if (Game.input.OnMousePress(MouseButton.Left))
            {
                int offset = 2;
                int x = (int)(camera.position.X + camera.forward.X * offset);
                int y = (int)(camera.position.Y + camera.forward.Y * offset);
                int z = (int)(camera.position.Z + camera.forward.Z * offset);

                game.world.AddBlockToWorld(x, y, z, BlockType.Air);
            }

            if (game.window.Focused)
            {
                camera.UpdatePitchAndYaw();
                camera.ResetCursor(game.window.Bounds);
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

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            } else if (isCrouching)
            {
                if (isInCreativeMode)
                {
                    MovePlayerVertically(-speedMultiplier);
                } else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }
            if (isInAir && !isInCreativeMode)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
            }

            if (inputToJump)
            {
                if (isInCreativeMode)
                {
                    MovePlayerVertically(speedMultiplier);
                } else
                {
                    AttemptToJump();
                }
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

            if (!HasPlayerSurpassedTerminalVelocity())
            {
                verticalSpeed += (float)(Constants.GRAVITY * deltaTime);
                MovePlayerVertically(verticalSpeed);
            } else
            {
                verticalSpeed = Constants.GRAVITY_THRESHOLD;
            }
        }

        private bool HasPlayerSurpassedTerminalVelocity()
        {
            return verticalSpeed < Constants.GRAVITY_THRESHOLD;
        }

        private void DoXAxisCollisionDetection()
        {
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(game.world))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.Intersects(blockAABB))
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
                    TryStopRunning();
                }
            }
        }

        private void DoYAxisCollisionDetection()
        {
            bool collidedY = false;
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(game.world))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.Intersects(blockAABB))
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

        private void DoZAxisCollisionDetection()
        {
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(game.world))
            {
                AABB blockAABB = Cube.GetAABB(collidablePos.X, collidablePos.Y, collidablePos.Z);
                if (hitbox.Intersects(blockAABB))
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

        private List<Vector3> GetCollisionDetectionBlockPositions(World world)
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
                        if (world.GetBlockAt(xx, yy, zz) != BlockType.Air)
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
