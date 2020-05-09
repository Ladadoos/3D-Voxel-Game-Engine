using OpenTK;

namespace Minecraft
{
    class Camera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public float Pitch { get; private set; }
        public float Yaw { get; private set; }

        private ViewFrustum viewFrustum;

        private readonly ProjectionMatrixInfo defaultProjection;
        public ProjectionMatrixInfo CurrentProjection { get; private set; }
        public Matrix4 CurrentProjectionMatrix { get; private set; }
        public Matrix4 CurrentViewMatrix { get; private set; }

        public delegate void OnProjectionChanged(ProjectionMatrixInfo info);
        public event OnProjectionChanged OnProjectionChangedHandler;

        public Camera(ProjectionMatrixInfo projectionInfo)
        {
            defaultProjection = projectionInfo.ShallowCopy();
            CurrentProjection = projectionInfo;
            CurrentProjectionMatrix = CreateProjectionMatrix();

            Position = new Vector3();
            Forward = new Vector3();
            viewFrustum = new ViewFrustum(projectionInfo);
        }

        public bool IsAABBInViewFrustum(AxisAlignedBox aabb)
        {
            return viewFrustum.IsAABBInFrustum(aabb);
        }

        public void SetFieldOfView(float fieldOfView)
        {
            if (CurrentProjection.fieldOfView != fieldOfView)
            {
                CurrentProjection.fieldOfView = fieldOfView;
                CurrentProjectionMatrix = CreateProjectionMatrix();
                OnProjectionChangedHandler?.Invoke(CurrentProjection);
            }
        }

        public void SetFieldOfViewToDefault()
        {
            SetFieldOfView(defaultProjection.fieldOfView);
        }

        public void SetWindowSize(int width, int height)
        {
            CurrentProjection.windowHeight = height;
            CurrentProjection.windowWidth = width;
            CurrentProjectionMatrix = CreateProjectionMatrix();
            OnProjectionChangedHandler?.Invoke(CurrentProjection);
        }

        private Matrix4 CreateProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(
                CurrentProjection.fieldOfView,
                CurrentProjection.windowWidth / (float)CurrentProjection.windowHeight,
                CurrentProjection.distanceNearPlane,
                CurrentProjection.distanceFarPlane);
        }

        private Matrix4 CreateViewMatrix()
        {
            Vector3 lookAt = Maths.CreateLookAtVector(Yaw, Pitch);
            return Matrix4.LookAt(Position, Position + lookAt, Vector3.UnitY);
        }

        public void SetPosition(Vector3 position)
        {
            this.Position = position;
        }

        public void SetYawAndPitch(float pitch, float yaw)
        {
            this.Pitch = pitch;
            this.Yaw = yaw;

            Forward = Maths.CreateLookAtVector(yaw, pitch);
            Right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, Forward));
        }

        public void Update()
        {
            viewFrustum.UpdateFrustumPoints(this);
            CurrentViewMatrix = CreateViewMatrix();
        }
    }
}
