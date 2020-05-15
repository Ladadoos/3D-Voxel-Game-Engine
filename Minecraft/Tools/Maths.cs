using OpenTK;
using System;

namespace Minecraft
{
    static class Maths
    {
        public static Matrix4 CreateTransformationMatrix(Vector3 translation,
                                                        float rx = 0, float ry = 0, float rz = 0,
                                                        float scaleX = 1, float scaleY = 1, float scaleZ = 1)
        {
            Matrix4 scaleMatrix = new Matrix4(scaleX, 0, 0, 0, 0, scaleY, 0, 0, 0, 0, scaleZ, 0, 0, 0, 0, 1);
            return scaleMatrix * CreateRotationAndTranslationMatrix(translation, new Vector3(rx, ry, rz));
        }

        public static Matrix4 CreateRotationAndTranslationMatrix(Vector3 translation, Vector3 rotation)
        {
            return Matrix4.CreateRotationX(DegreeToRadian(rotation.X)) *
                Matrix4.CreateRotationY(DegreeToRadian(rotation.Y)) *
                Matrix4.CreateRotationZ(DegreeToRadian(rotation.Z)) *
                Matrix4.CreateTranslation(translation);
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

        /// <summary>
        /// Converts from one range to another. Boundaries are all inclusive.
        /// </summary>
        public static float ConvertRange(float oldMin, float oldMax, float newMin, float newMax, float oldValue)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;
            return (((oldValue - oldMin) * newRange) / oldRange) + newMin;
        }
    }
}
