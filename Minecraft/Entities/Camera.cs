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
        public float pitch, yaw;

        private Vector2 lastMousePos;
        private GameWindow window;

        public ViewFrustum viewFrustum { get; private set; }

        private ProjectionMatrixInfo defaultProjection;
        public ProjectionMatrixInfo currentProjection { get; private set; }
        public Matrix4 currentProjectionMatrix { get; private set; }
        public Matrix4 currentViewMatrix { get; private set; }

        public delegate void OnProjectionChanged(ProjectionMatrixInfo info);
        public event OnProjectionChanged OnProjectionChangedHandler;

        public Camera(GameWindow window, ProjectionMatrixInfo projectionInfo)
        {
            this.window = window;

            defaultProjection = projectionInfo.ShallowCopy();
            currentProjection = projectionInfo;
            currentProjectionMatrix = CreateProjectionMatrix();

            position = new Vector3();
            forward = new Vector3();
            lastMousePos = new Vector2();
            viewFrustum = new ViewFrustum(projectionInfo);
        }

        public void SetFieldOfView(float fieldOfView)
        {
            if (currentProjection.fieldOfView != fieldOfView)
            {
                currentProjection.fieldOfView = fieldOfView;
                currentProjectionMatrix = CreateProjectionMatrix();
                OnProjectionChangedHandler?.Invoke(currentProjection);
            }
        }

        public void SetFieldOfViewToDefault()
        {
            SetFieldOfView(defaultProjection.fieldOfView);
        }

        public void SetWindowSize(int width, int height)
        {
            currentProjection.windowHeight = height;
            currentProjection.windowWidth = width;
            currentProjectionMatrix = CreateProjectionMatrix();
            OnProjectionChangedHandler?.Invoke(currentProjection);
        }

        private Matrix4 CreateProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                currentProjection.fieldOfView,
                currentProjection.windowWidth / (float)currentProjection.windowHeight,
                currentProjection.distanceNearPlane,
                currentProjection.distanceFarPlane);
        }

        private Matrix4 CreateViewMatrix()
        {
            Vector3 lookAt = Maths.CreateLookAtVector(yaw, pitch);
            return Matrix4.LookAt(position, position + lookAt, Vector3.UnitY);
        }

        public void SetPosition(Vector3 position)
        {
            this.position = position;
        }

        public void Update()
        {
            UpdatePitchAndYaw();
            viewFrustum.UpdateFrustumPoints(this);
            ResetCursorToWindowCenter();
            currentViewMatrix = CreateViewMatrix();
        }

        private void UpdatePitchAndYaw()
        {
            Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            delta.X = delta.X * Constants.PLAYER_MOUSE_SENSIVITY;
            delta.Y = delta.Y * Constants.PLAYER_MOUSE_SENSIVITY;

            pitch = (pitch + delta.X) % ((float)Math.PI * 2.0F);
            yaw = Math.Max(Math.Min(yaw + delta.Y, (float)Math.PI / 2.0F - 0.1F), (float)-Math.PI / 2.0F + 0.1F);

            forward = Maths.CreateLookAtVector(yaw, pitch);
            right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
        }

        private void ResetCursorToWindowCenter()
        {
            Rectangle bounds = window.Bounds;
            Mouse.SetPosition(bounds.Left + bounds.Width / 2.0D, bounds.Top + bounds.Height / 2.0D);
            lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }
    }
}
