using OpenTK;

namespace Minecraft
{
    class Camera
    {
        public Vector3 position { get; private set; }
        public Vector3 forward { get; private set; }
        public Vector3 right { get; private set; }
        public float pitch { get; private set; }
        public float yaw { get; private set; }

        public ViewFrustum viewFrustum { get; private set; }

        private ProjectionMatrixInfo defaultProjection;
        public ProjectionMatrixInfo currentProjection { get; private set; }
        public Matrix4 currentProjectionMatrix { get; private set; }
        public Matrix4 currentViewMatrix { get; private set; }

        public delegate void OnProjectionChanged(ProjectionMatrixInfo info);
        public event OnProjectionChanged OnProjectionChangedHandler;

        public Camera(ProjectionMatrixInfo projectionInfo)
        {
            defaultProjection = projectionInfo.ShallowCopy();
            currentProjection = projectionInfo;
            currentProjectionMatrix = CreateProjectionMatrix();

            position = new Vector3();
            forward = new Vector3();
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

        public void SetYawAndPitch(float pitch, float yaw)
        {
            this.pitch = pitch;
            this.yaw = yaw;

            forward = Maths.CreateLookAtVector(yaw, pitch);
            right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
        }

        public void Update()
        {
            viewFrustum.UpdateFrustumPoints(this);
            currentViewMatrix = CreateViewMatrix();
        }
    }
}
