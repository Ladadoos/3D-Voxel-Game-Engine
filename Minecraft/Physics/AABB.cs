using OpenTK;

namespace Minecraft
{
    class AABB
    {
        public Vector3 max;
        public Vector3 min;

        public AABB(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public void setHitbox(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }

        public bool intersects(AABB c)
        {
            return min.X < c.max.X && max.X > c.min.X &&
                   min.Y < c.max.Y && max.Y > c.min.Y &&
                   min.Z < c.max.Z && max.Z > c.min.Z;
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
    }
}
