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

        private MouseRay mouseRay;

        private BlockType selectedBlock = BlockType.Dirt;

        public Player(Game game, Matrix4 projectionMatrix)
        {
            this.game = game;
            camera = new Camera();
            position = new Vector3(Constants.CHUNK_SIZE / 2, 148, Constants.CHUNK_SIZE / 2);
            mouseRay = new MouseRay(camera, projectionMatrix);
            hitbox = new AABB(position, GetPlayerMaxAABB());
        }

        public void Update(float deltaTime)
        {
            if (GameWindow.instance.Focused)
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
           
            camera.SetPosition(position);
            mouseRay.Update();
            if (Game.input.OnMousePress(MouseButton.Right))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                game.world.AddBlockToWorld(x, y, z, selectedBlock);
            }
            if (Game.input.OnMousePress(MouseButton.Middle))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                selectedBlock = game.world.GetBlockAt(x, y, z);
            }
            if (Game.input.OnMousePress(MouseButton.Left))
            {
                int offset = 2;
                int x = (int)(camera.position.X + mouseRay.currentRay.X * offset);
                int y = (int)(camera.position.Y + mouseRay.currentRay.Y * offset);
                int z = (int)(camera.position.Z + mouseRay.currentRay.Z * offset);

                game.world.AddBlockToWorld(x, y, z, BlockType.Air);
            }

            if (GameWindow.instance.Focused)
            {
                camera.Rotate();
                camera.ResetCursor(GameWindow.instance.Bounds);
            }      
        }

        private void TryStartRunning()
        {
            if(!isRunning && (isInCreativeMode || (!isInCreativeMode && !isInAir)))
            {
                isRunning = true;
                game.masterRenderer.SetFieldOfView(1.65F);
            }
        }

        private void TryStopRunning()
        {
            if (isRunning)
            {
                isRunning = false;
                game.masterRenderer.ResetToDefaultFieldOfView();
            }
        }

        private void TryStopCrouching()
        {
            if (isCrouching)
            {
                isCrouching = false;
                game.masterRenderer.ResetToDefaultFieldOfView();
            }
        }

        private void TryStartCrouching()
        {
            if (isRunning)
            {
                TryStopRunning();
            }

            game.masterRenderer.SetFieldOfView(1.45F);
            isCrouching = true;
        }

        private void UpdateKeyboardInput()
        {
            speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            bool inputToRun = Game.input.OnKeyPress(Key.ControlLeft) || Game.input.OnKeyPress(Key.ControlRight);
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
            }
            else if (inputToCrouch)
            {
                TryStartCrouching();
            }else if (!inputToCrouch)
            {
                TryStopCrouching();
            }

            if(!inputToMoveForward || inputToMoveBack)
            {
                TryStopRunning();
            }

            if (isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            }else if (isCrouching)
            {
                if (isInCreativeMode)
                {
                    ApplyForce(0.0F, -1.0F * speedMultiplier, 0.0F);
                }
                else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }
            /*if (isInAir && !isInCreativeMode)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
            }*/

            if (inputToJump)
            {
                if (isInCreativeMode)
                {
                    ApplyForce(0.0F, 1.0F * speedMultiplier, 0.0F);
                }
                else
                {
                    AttemptToJump();
                }
            }

            if (inputToMoveForward)
            {
                ApplyForce(0.0F, 0.0F, 1.0F * speedMultiplier);
            }
            if (inputToMoveBack)
            {
                ApplyForce(0.0F, 0.0F, -1.0F * speedMultiplier);
            }
            if (inputToMoveRight)
            {
                ApplyForce(1.0F * speedMultiplier, 0.0F, 0.0F);
            }
            if (inputToMoveLeft)
            {
                ApplyForce(-1.0F * speedMultiplier, 0.0F, 0.0F);
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
                ApplyForce(0.0F, verticalSpeed, 0.0F);
            }
            else
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

        private void DoZAxisCollisionDetection()
        {
            foreach (Vector3 collidablePos in GetCollisionDetectionBlockPositions(game.world))
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
                    TryStopRunning();
                }
            }
        }

        private void ApplyForce(float x, float y, float z)
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

        private void CalculateAABBHitbox()
        {
            hitbox.setHitbox(position, GetPlayerMaxAABB());
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
