using OpenTK;
using OpenTK.Input;

namespace Minecraft
{
    class MouseRay
    {
        public Vector3 currentRay;
        private Matrix4 projectionMatrix;
        private Matrix4 viewMatrix;
        private Camera camera;

        public MouseRay(Camera cam, Matrix4 projection)
        {
            camera = cam;
            projectionMatrix = projection;
            viewMatrix = Maths.CreateViewMatrix(cam);
        }

        public void Update()
        {
            viewMatrix = Maths.CreateViewMatrix(camera);
            currentRay = CalculateMouseRay();
        }

        private Vector3 CalculateMouseRay()
        {
            float mouseX = Mouse.GetCursorState().X;
            float mouseY = Mouse.GetCursorState().Y;
            Vector2 normalizedCoords = GetNormalizedDeviceCoords(mouseX, mouseY);
            Vector4 clipCoords = new Vector4(normalizedCoords.X, normalizedCoords.Y, -1.0f, 1.0f);
            Vector4 eyeCoords = ToEyeCoords(clipCoords);
            Vector3 worldRay = ToWorldCoords(eyeCoords);
            return worldRay;
        }

        private Vector3 ToWorldCoords(Vector4 eyeCoords)
        {
            Matrix4 invertedView = Matrix4.Invert(viewMatrix);
            Vector4 rayWorld = Vector4.Transform(eyeCoords, invertedView);
            Vector3 mouseRay = new Vector3(rayWorld.X, rayWorld.Y, rayWorld.Z);
            mouseRay.Normalize();
            return mouseRay;
        }

        private Vector4 ToEyeCoords(Vector4 clipCoords)
        {
            Matrix4 invertedProjection = Matrix4.Invert(projectionMatrix);
            Vector4 eyeCoords = Vector4.Transform(clipCoords, invertedProjection);
            return new Vector4(eyeCoords.X, eyeCoords.Y, -1.0f, 0.0f);
        }

        private Vector2 GetNormalizedDeviceCoords(float mouseX, float mouseY)
        {
            float x = (2.0f * mouseX) / DisplayDevice.GetDisplay(DisplayIndex.First).Width - 1;
            float y = (2.0f * mouseY) / DisplayDevice.GetDisplay(DisplayIndex.First).Height - 1;
            return new Vector2(x, y);
        }
    }
}
