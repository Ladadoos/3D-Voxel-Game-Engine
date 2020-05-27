using OpenTK;
using System;

namespace Minecraft
{
    static class Vector2Extensions
    {
        public static int ManhattanDistance(this Vector2 thisVec, Vector2 otherVec)
        {
            return (int)(Math.Abs(thisVec.X - otherVec.X) + Math.Abs(thisVec.Y - otherVec.Y));
        }
    }
}
