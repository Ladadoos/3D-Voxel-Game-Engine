using OpenTK;
using System;

namespace Minecraft
{
    static class Vector2Extensions
    {
        public static Vector2 ManhattanDistance(this Vector2 thisVec, Vector2 otherVec)
        {
            return new Vector2(Math.Abs(thisVec.X - otherVec.X) + Math.Abs(thisVec.Y - otherVec.Y);
        }
    }
}
