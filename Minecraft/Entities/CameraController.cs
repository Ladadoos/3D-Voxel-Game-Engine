using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;

namespace Minecraft
{
    class CameraController
    {
        public Camera Camera { get; private set; }

        private readonly GameWindow window;
        private Vector2 lastMousePos = new Vector2();

        public CameraController(GameWindow window)
        {
            this.window = window;
        }

        public void Update()
        {
            Camera.Update();
            if (!window.Focused)
            {
                return;
            }
            UpdateCameraPitchAndYaw();
            ResetCursorToWindowCenter();
        }

        public void ControlCamera(Camera camera)
        {
            this.Camera = camera;
        }

        private void ResetCursorToWindowCenter()
        {
            Rectangle bounds = window.Bounds;
            Mouse.SetPosition(bounds.Left + bounds.Width / 2.0D, bounds.Top + bounds.Height / 2.0D);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        private void UpdateCameraPitchAndYaw()
        {
            Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            delta.X = delta.X * Constants.PLAYER_MOUSE_SENSIVITY;
            delta.Y = delta.Y * Constants.PLAYER_MOUSE_SENSIVITY;

            float newPitch = (Camera.Pitch + delta.X) % ((float)Math.PI * 2.0F);
            float newYaw = Math.Max(Math.Min(Camera.Yaw + delta.Y, (float)Math.PI / 2.0F - 0.1F), (float)-Math.PI / 2.0F + 0.1F);
            Camera.SetYawAndPitch(newPitch, newYaw);
        }
    }
}
