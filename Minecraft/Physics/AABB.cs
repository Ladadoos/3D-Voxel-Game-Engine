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
            if ((c.max.X <= min.X) || (c.min.X >= max.X)) return false;
            if ((c.max.Y <= min.Y) || (c.min.Y >= max.Y)) return false;
            if ((c.max.Z <= min.Z) || (c.min.Z >= max.Z)) return false;
            return true;
        }
    }
}
