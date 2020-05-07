using OpenTK;
using System;

namespace Minecraft
{
    class AxisAlignedBox
    {
        public Vector3 min { get; private set; }
        public Vector3 max { get; private set; }

        public AxisAlignedBox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public void SetDimensions(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Intersects(AxisAlignedBox c)
        {
            return min.X < c.max.X && max.X > c.min.X &&
                   min.Y < c.max.Y && max.Y > c.min.Y &&
                   min.Z < c.max.Z && max.Z > c.min.Z;
        }

        public float Intersects(Ray ray)
        {
            float t1 = (min.X - ray.origin.X) * ray.directionFrac.X;
            float t2 = (max.X - ray.origin.X) * ray.directionFrac.X;
            float t3 = (min.Y - ray.origin.Y) * ray.directionFrac.Y;
            float t4 = (max.Y - ray.origin.Y) * ray.directionFrac.Y;
            float t5 = (min.Z - ray.origin.Z) * ray.directionFrac.Z;
            float t6 = (max.Z - ray.origin.Z) * ray.directionFrac.Z;

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

            if(ray.distanceToIntersection > tmin)
            {
                return tmin;
            }
            return float.MaxValue;
        }

        public Vector3[] GetAllCorners()
        {
            Vector3[] allCorners = new Vector3[8];
            Vector3 deltaX = new Vector3(max.X - min.X, 0, 0);
            Vector3 deltaZ = new Vector3(0, 0, max.Z - min.Z);

            allCorners[0] = min;
            allCorners[1] = min + deltaX;
            allCorners[2] = min + deltaZ;
            allCorners[3] = min + deltaX + deltaZ;

            allCorners[4] = max;
            allCorners[5] = max - deltaX;
            allCorners[6] = max - deltaZ;
            allCorners[7] = max - deltaX - deltaZ;

            return allCorners;
        }
     
        public Vector3 GetNormalAtIntersectionPoint(Vector3 point)
        {
            float bias = 1.00005f;
            Vector3 c = (min + max) * 0.5f;
            Vector3 p = point - c;
            Vector3 d = (min - max) * 0.5f;
            return (new Vector3(
                (int)(p.X / Math.Abs(d.X) * bias),
                (int)(p.Y / Math.Abs(d.Y) * bias),
                (int)(p.Z / Math.Abs(d.Z) * bias))).Normalized();
        }
    }
}
