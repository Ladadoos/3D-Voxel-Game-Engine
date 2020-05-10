using System;
using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class ClientPlayer : Player
    {
        private readonly Game game;

        public Camera camera;
        public RayTraceResult mouseOverObject { get; private set; }

        private BlockState selectedBlock = Blocks.SugarCane.GetNewDefaultState();

        private const float secondsPerPosUpdate = 0.05F;
        private float elapsedMsSinceLastPosUpdate;

        public ClientPlayer(Game game) : base(-1, "", new Vector3(0, 200, 0))
        {
            this.game = game;
            camera = new Camera(new ProjectionMatrixInfo {
                DistanceNearPlane = 0.1F,
                DistanceFarPlane = 1000F,
                FieldOfView = 1.5F,
                WindowPixelWidth = game.Window.Width,
                WindowPixelHeight = game.Window.Height
            });

            OnToggleRunningHandler += OnRunningToggle;
            OnToggleCrouchingHandler += OnCrouchingToggle;
        }

        private void OnRunningToggle(bool isRunning)
        {
            if(isRunning)
            {
                camera.SetFieldOfView(1.65F);
            } else
            {
                camera.SetFieldOfViewToDefault();
            }
        }

        private void OnCrouchingToggle(bool isCrouching)
        {
            if(isCrouching)
            {
                camera.SetFieldOfView(1.45F);
            } else
            {
                camera.SetFieldOfViewToDefault();
            }
        }

        private void UpdateCameraPosition()
        {
            Vector3 cameraPosition = Position;
            cameraPosition.X += Constants.PLAYER_WIDTH / 2.0F;
            cameraPosition.Y += Constants.PLAYER_CAMERA_HEIGHT;
            cameraPosition.Z += Constants.PLAYER_LENGTH / 2.0F;
            camera.SetPosition(cameraPosition);
        }

        public override void Update(float deltaTime, World world)
        {
            Acceleration = Vector3.Zero;
            UpdateKeyboardInput();
            ApplyVelocityAndCheckCollision(deltaTime, world);
            mouseOverObject = new Ray(camera.Position, camera.Forward).TraceWorld(world);

            UpdateCameraPosition();

            if(Game.Input.OnMousePress(MouseButton.Right) && game.Window.Focused && mouseOverObject != null)
            {
                if(!isCrouching && world.GetBlockAt(mouseOverObject.IntersectedBlocKPos).GetBlock().isInteractable)
                {
                    game.Client.WritePacket(new PlayerBlockInteractionPacket(mouseOverObject.IntersectedBlocKPos));
                } else if(selectedBlock.GetBlock().CanAddBlockAt(world, mouseOverObject.BlockPlacePosition))
                {
                    BlockState newBlock = selectedBlock.GetBlock().GetNewDefaultState();
                    game.Client.WritePacket(new PlaceBlockPacket(newBlock, mouseOverObject.BlockPlacePosition));
                }
            }
            if(Game.Input.OnMousePress(MouseButton.Middle) && mouseOverObject != null)
            {
                selectedBlock = world.GetBlockAt(mouseOverObject.IntersectedBlocKPos);
            }
            if(Game.Input.OnMousePress(MouseButton.Left) && mouseOverObject != null)
            {
                game.Client.WritePacket(new RemoveBlockPacket(mouseOverObject.IntersectedBlocKPos));
            }

            realForward = camera.Forward;
            moveForward = new Vector3((float)Math.Sin(camera.Pitch), 0, (float)Math.Cos(camera.Pitch));
            right = camera.Right;

            elapsedMsSinceLastPosUpdate += deltaTime;
            if(elapsedMsSinceLastPosUpdate > secondsPerPosUpdate)
            {
                elapsedMsSinceLastPosUpdate = 0;
                game.Client.WritePacket(new PlayerDataPacket(Position, ID));
            }
        }

        private void UpdateKeyboardInput()
        {
            float speedMultiplier = Constants.PLAYER_BASE_MOVE_SPEED;

            bool wFocused = game.Window.Focused;
            bool inputToRun = wFocused && (Game.Input.OnKeyDown(Key.ControlLeft) || Game.Input.OnKeyDown(Key.ControlRight));
            bool inputToCrouch = wFocused && (Game.Input.OnKeyDown(Key.ShiftLeft) || Game.Input.OnKeyDown(Key.ShiftRight));
            bool inputToMoveLeft = wFocused && Game.Input.OnKeyDown(Key.A);
            bool inputToMoveBack = wFocused && Game.Input.OnKeyDown(Key.S);
            bool inputToMoveRight = wFocused && Game.Input.OnKeyDown(Key.D);
            bool inputToMoveForward = wFocused && Game.Input.OnKeyDown(Key.W);
            bool inputToJump = wFocused && Game.Input.OnKeyDown(Key.Space);
            bool inputToFly = wFocused && Game.Input.OnKeyPress(Key.Space);

            //Prioritize crouching over running
            if(inputToRun && !inputToCrouch)
            {
                TryStartRunning();
            } else if(inputToCrouch)
            {
                TryStartCrouching();
            } else if(!inputToCrouch)
            {
                TryStopCrouching();
            }

            if(!inputToMoveForward || inputToMoveBack)
            {
                TryStopRunning();
            }

            if(isInAir && !isFlying)
            {
                speedMultiplier *= Constants.PLAYER_IN_AIR_SLOWDOWN;
            }
            if(isFlying)
            {
                speedMultiplier *= Constants.PLAYER_FLYING_MULTIPLIER;
            }

            if(isRunning)
            {
                speedMultiplier *= Constants.PLAYER_SPRINT_MULTIPLIER;
            } else if(isCrouching)
            {
                if(isFlying)
                {
                    MovePlayerVertically(-speedMultiplier);
                } else
                {
                    speedMultiplier *= Constants.PLAYER_CROUCH_MULTIPLIER;
                }
            }

            if(inputToJump)
            {
                if(isFlying)
                {
                    MovePlayerVertically(speedMultiplier);
                } else
                {
                    AttemptToJump();
                }
            }

            if(inputToFly)
            {
                TryToggleFlying();
            }

            if(inputToMoveForward)
            {
                MovePlayerHorizontally(0, speedMultiplier);
            }
            if(inputToMoveBack)
            {
                MovePlayerHorizontally(0, -speedMultiplier);
            }
            if(inputToMoveRight)
            {
                MovePlayerHorizontally(-speedMultiplier, 0);
            }
            if(inputToMoveLeft)
            {
                MovePlayerHorizontally(speedMultiplier, 0);
            }
        }
    }
}
