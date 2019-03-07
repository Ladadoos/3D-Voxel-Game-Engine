using OpenTK;
using System;

namespace Minecraft
{
    class Maths
    {
        public static Matrix4 CreateTransformationMatrix(Vector3 translation, 
                                                        float rx = 0, float ry = 0, float rz = 0,
                                                        float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            Matrix4 scaleMatrix = new Matrix4(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);
            Matrix4 transformationmatrix = scaleMatrix *
                Matrix4.CreateRotationX(ToRadians(rx)) *
                Matrix4.CreateRotationY(ToRadians(ry)) *
                Matrix4.CreateRotationZ(ToRadians(rz)) *
                Matrix4.CreateTranslation(translation);
            return transformationmatrix;
        }

        public static Matrix4 CreateProjectionMatrix(float fov, float width, float height, float near, float far)
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, width / height, near, far);
        }

        public static Matrix4 CreateViewMatrix(Camera camera)
        {
            Vector3 lookAt = CreateLookAtVector(camera);
            return Matrix4.LookAt(camera.position, camera.position + lookAt, Vector3.UnitY);
        }

        public static Vector3 CreateLookAtVector(Camera camera)
        {
            Vector3 lookAt = new Vector3();
            lookAt.X = (float)(Math.Sin(camera.radialOrientation.X) * Math.Cos(camera.radialOrientation.Y));
            lookAt.Y = (float)Math.Sin(camera.radialOrientation.Y);
            lookAt.Z = (float)(Math.Cos(camera.radialOrientation.X) * Math.Cos(camera.radialOrientation.Y));
            return lookAt;
        }

        public static float ToRadians(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }
    }
}
