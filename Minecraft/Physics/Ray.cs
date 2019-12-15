using OpenTK;

namespace Minecraft
{
    class Ray
    {
        public Vector3 origin;
        public Vector3 direction;
        public float distanceToIntersection;

        public Vector3 directionFrac;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction.Normalized();
            directionFrac = new Vector3(1 / this.direction.X, 1 / this.direction.Y, 1 / this.direction.Z);
            distanceToIntersection = float.MaxValue;
        }

        public RayTraceResult TraceWorld(World world)
        {
            Vector3 position = origin;
            Vector3 offset = direction / 25;
            BlockState targetBlock = null;
            for (int i = 0; i < 100; i++)
            {
                position += offset;
                targetBlock = world.GetBlockAt(position);
                if (targetBlock.block != Blocks.Air)
                {
                    break;
                }
            }

            if (targetBlock == null)
            {
                return null;
            }

            AABB targetAABB = Block.GetFullBlockCollision(targetBlock);
            float dist = targetAABB.Intersects(this);
            if (dist == float.MaxValue)
            {
                return null;
            }
            this.distanceToIntersection = dist;
            Vector3 interPoint = origin + direction * distanceToIntersection;
            return new RayTraceResult(targetAABB, targetAABB.GetNormalAtIntersectionPoint(interPoint), interPoint, targetBlock.position);
        }
    }
}
