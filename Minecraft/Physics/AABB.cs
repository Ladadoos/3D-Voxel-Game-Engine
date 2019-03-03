using OpenTK;

namespace Minecraft.Physics
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
    }
}
