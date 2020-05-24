using OpenTK;

namespace Minecraft
{
    static class Vector3Extensions
    {
        public static Vector3 Plus(this Vector3 vector, Vector3i vectorI)
        {
            return new Vector3(vector.X + vectorI.X, vector.Y + vectorI.Y, vector.Z + vectorI.Z);
        }

        public static Vector3 Plus(this Vector3 vector, float value)
        {
            return new Vector3(vector.X + value, vector.Y + value, vector.Z + value);
        }
    }
}
