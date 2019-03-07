using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;

namespace Minecraft
{
    class Camera
    {
        public Vector3 position;
        public Vector3 rayOut;
        public Vector3 radialOrientation;

        private Vector2 lastMousePos;

        public Camera()
        {
            position = new Vector3();
            radialOrientation = new Vector3();
            lastMousePos = new Vector2();
            rayOut = new Vector3();
        }

        public void SetPosition(Vector3 playerPos)
        {
            position.X = playerPos.X + Constants.PLAYER_WIDTH / 2.0F;
            position.Y = playerPos.Y + Constants.PLAYER_CAMERA_HEIGHT;
            position.Z = playerPos.Z + Constants.PLAYER_LENGTH / 2.0F;
        }

        public void Rotate()
        {
            Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            delta.X = delta.X * Constants.PLAYER_MOUSE_SENSIVITY;
            delta.Y = delta.Y * Constants.PLAYER_MOUSE_SENSIVITY;

            radialOrientation.X = (radialOrientation.X + delta.X) % ((float)Math.PI * 2.0F);
            radialOrientation.Y = Math.Max(Math.Min(radialOrientation.Y + delta.Y, (float)Math.PI / 2.0F - 0.1F), (float)-Math.PI / 2.0F + 0.1F);

            CalculateRayOut();
        }

        private void CalculateRayOut()
        {
            double cosY = Math.Cos(radialOrientation.Y);
            rayOut = new Vector3((float)(cosY * Math.Sin(radialOrientation.X)),
                                 (float)Math.Sin(radialOrientation.Y),
                                 (float)(cosY * Math.Cos(radialOrientation.X)));
        }

        public void ResetCursor(Rectangle bounds)
        {
            Mouse.SetPosition(bounds.Left + bounds.Width / 2.0D, bounds.Top + bounds.Height / 2.0D);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}
