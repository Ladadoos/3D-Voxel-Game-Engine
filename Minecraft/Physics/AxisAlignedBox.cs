using OpenTK;
using System;

namespace Minecraft
{
    class AxisAlignedBox
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public AxisAlignedBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public void SetDimensions(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public bool Intersects(AxisAlignedBox c)
        {
            return Min.X < c.Max.X && Max.X > c.Min.X &&
                   Min.Y < c.Max.Y && Max.Y > c.Min.Y &&
                   Min.Z < c.Max.Z && Max.Z > c.Min.Z;
        }

        public float Intersects(Ray ray)
        {
            float t1 = (Min.X - ray.Origin.X) * ray.DirectionFrac.X;
            float t2 = (Max.X - ray.Origin.X) * ray.DirectionFrac.X;
            float t3 = (Min.Y - ray.Origin.Y) * ray.DirectionFrac.Y;
            float t4 = (Max.Y - ray.Origin.Y) * ray.DirectionFrac.Y;
            float t5 = (Min.Z - ray.Origin.Z) * ray.DirectionFrac.Z;
            float t6 = (Max.Z - ray.Origin.Z) * ray.DirectionFrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            // if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
            if(tmax < 0)
            {
                return float.MaxValue;
            }

            // if tmin > tmax, ray doesn't intersect AABB
            if(tmin > tmax)
            {
                return float.MaxValue;
            }

            if(ray.DistanceToIntersection > tmin)
            {
                return tmin;
            }
            return float.MaxValue;
        }

        public Vector3[] GetAllCorners()
        {
            Vector3[] allCorners = new Vector3[8];
            Vector3 deltaX = new Vector3(Max.X - Min.X, 0, 0);
            Vector3 deltaZ = new Vector3(0, 0, Max.Z - Min.Z);

            allCorners[0] = Min;
            allCorners[1] = Min + deltaX;
            allCorners[2] = Min + deltaZ;
            allCorners[3] = Min + deltaX + deltaZ;

            allCorners[4] = Max;
            allCorners[5] = Max - deltaX;
            allCorners[6] = Max - deltaZ;
            allCorners[7] = Max - deltaX - deltaZ;

            return allCorners;
        }
     
        public Vector3 GetNormalAtIntersectionPoint(Vector3 point)
        {
            float bias = 1.00005f;
            Vector3 c = (Min + Max) * 0.5f;
            Vector3 p = point - c;
            Vector3 d = (Min - Max) * 0.5f;
            return (new Vector3(
                (int)(p.X / Math.Abs(d.X) * bias),
                (int)(p.Y / Math.Abs(d.Y) * bias),
                (int)(p.Z / Math.Abs(d.Z) * bias))).Normalized();
        }
    }
}
