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
            Vector3 targetPos = Vector3.Zero;
            bool foundTarget = false;
            for (int i = 0; i < 80; i++)
            {
                position += offset;
                int intX = (int)position.X;
                int intY = (int)position.Y;
                int intZ = (int)position.Z;
                if (world.GetBlockAt(intX, intY, intZ) != BlockType.Air)
                {
                    targetPos = new Vector3(intX, intY, intZ);
                    foundTarget = true;
                    break;
                }
            }

            if (!foundTarget)
            {
                return null;
            }

            AABB targetAABB = Cube.GetAABB(targetPos.X, targetPos.Y, targetPos.Z);
            float dist = targetAABB.Intersects(this);
            if (dist == float.MaxValue)
            {
                return null;
            }
            this.distanceToIntersection = dist;
            Vector3 interPoint = origin + direction * distanceToIntersection;
            return new RayTraceResult(targetAABB, targetAABB.GetNormalAtIntersectionPoint(interPoint), interPoint, targetPos);
        }
    }
}
