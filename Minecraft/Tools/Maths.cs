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
                Matrix4.CreateRotationX(DegreeToRadian(rx)) *
                Matrix4.CreateRotationY(DegreeToRadian(ry)) *
                Matrix4.CreateRotationZ(DegreeToRadian(rz)) *
                Matrix4.CreateTranslation(translation);
            return transformationmatrix;
        }

        public static Vector3 CreateLookAtVector(float yaw, float pitch)
        {
            Vector3 lookAt = new Vector3();
            double cosY = Math.Cos(yaw);
            lookAt.X = (float)(Math.Sin(pitch) * cosY);
            lookAt.Y = (float)Math.Sin(yaw);
            lookAt.Z = (float)(Math.Cos(pitch) * cosY);
            return lookAt;
        }

        public static float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
        {
            return from + (to - from) * t;
        }
    }
}
