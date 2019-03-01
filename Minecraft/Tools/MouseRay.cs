using Minecraft.Entities;
using OpenTK;
using OpenTK.Input;

namespace Minecraft.Tools
{
    class MouseRay
    {
        //  private const float RAY_RANGE = 600;
        //  public Vector3 currentRay;

        public Ray ray;
        private Matrix4 projectionMatrix;
        private Matrix4 viewMatrix;
        private Camera camera;

        public MouseRay(Camera cam, Matrix4 projection)
        {
            camera = cam;
            projectionMatrix = projection;
            viewMatrix = Maths.CreateViewMatrix(cam);
            ray = new Ray();
        }

        public void Update()
        {
            viewMatrix = Maths.CreateViewMatrix(camera);
            ray.currentRay = CalculateMouseRay();
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


        /*public bool CheckLineBox(Vector3 B1, Vector3 B2, Vector3 L1, Vector3 L2, ref Vector3 Hit) {
            if (L2.X < B1.X && L1.X < B1.X) return false;
            if (L2.X > B2.X && L1.X > B2.X) return false;
            if (L2.Y < B1.Y && L1.Y < B1.Y) return false;
            if (L2.Y > B2.Y && L1.Y > B2.Y) return false;
            if (L2.Z < B1.Z && L1.Z < B1.Z) return false;
            if (L2.Z > B2.Z && L1.Z > B2.Z) return false;
            if (L1.X > B1.X && L1.X < B2.X &&
                L1.Y > B1.Y && L1.Y < B2.Y &&
                L1.Z > B1.Z && L1.Z < B2.Z) {
                Hit = L1;
                return true;
            }
            if ((GetIntersection(L1.X - B1.X, L2.X - B1.X, L1, L2, ref Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B1.Y, L2.Y - B1.Y, L1, L2, ref Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B1.Z, L2.Z - B1.Z, L1, L2, ref Hit) && InBox(Hit, B1, B2, 3))
              || (GetIntersection(L1.X - B2.X, L2.X - B2.X, L1, L2, ref Hit) && InBox(Hit, B1, B2, 1))
              || (GetIntersection(L1.Y - B2.Y, L2.Y - B2.Y, L1, L2, ref Hit) && InBox(Hit, B1, B2, 2))
              || (GetIntersection(L1.Z - B2.Z, L2.Z - B2.Z, L1, L2, ref Hit) && InBox(Hit, B1, B2, 3)))
                return true;

            return false;
        }

        private bool GetIntersection(float fDst1, float fDst2, Vector3 P1, Vector3 P2, ref Vector3 Hit) {
            if ((fDst1 * fDst2) >= 0.0f) return false;
            if (fDst1 == fDst2) return false;
            Hit = P1 + (P2 - P1) * (-fDst1 / (fDst2 - fDst1));
            return true;
        }

        private bool InBox(Vector3 Hit, Vector3 B1, Vector3 B2, int Axis) {
            if (Axis == 1 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            if (Axis == 2 && Hit.Z > B1.Z && Hit.Z < B2.Z && Hit.X > B1.X && Hit.X < B2.X) return true;
            if (Axis == 3 && Hit.X > B1.X && Hit.X < B2.X && Hit.Y > B1.Y && Hit.Y < B2.Y) return true;
            return false;
        }*/

    }
}
