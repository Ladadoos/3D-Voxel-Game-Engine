using OpenTK;
using OpenTK.Input;
using System;
using System.Drawing;

namespace Minecraft
{
    class Camera
    {
        public Vector3 position;// { get; private set; }
        public Vector3 forward;// { get; private set; }
        public Vector3 right;// { get; private set; }
        public Vector3 radialOrientation;// { get; private set; }

        public ViewFrustum viewFrustum;

        private Vector2 lastMousePos;

        public Camera(Game game)
        {
            position = new Vector3();
            forward = new Vector3();
            radialOrientation = new Vector3();
            lastMousePos = new Vector2();
            ProjectionMatrixInfo info = game.masterRenderer.currentProjectionMatrixInfo;
            viewFrustum = new ViewFrustum(1.5F, info.windowWidth / info.windowHeight, 0.1F, 10000.0F);
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

            forward = Maths.CreateLookAtVector(this);
            right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));

            viewFrustum.UpdateFrustumPoints(this);
        }

        public void ResetCursor(Rectangle bounds)
        {
            Mouse.SetPosition(bounds.Left + bounds.Width / 2.0D, bounds.Top + bounds.Height / 2.0D);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}
