using OpenTK;

namespace Minecraft
{
    class Ray
    {
        public Vector3 origin { get; private set; }
        public Vector3 direction { get; private set; }
        public float distanceToIntersection { get; private set; }

        public Vector3 directionFrac { get; private set; }

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
            Vector3 offset = direction / 50;
            BlockState hitBlockState = null;
            for (int i = 0; i < 200; i++)
            {
                position += offset;
                hitBlockState = world.GetBlockAt(new Vector3i(position));
                if (hitBlockState.GetBlock() != Blocks.Air)
                {
                    break;
                }
            }

            if (hitBlockState == null || (hitBlockState != null && hitBlockState.GetBlock() == Blocks.Air))
            {
                return null;
            }

            Vector3i blockPos = new Vector3i(position);
            AxisAlignedBox[] hitAABBs = hitBlockState.GetBlock().GetSelectionBox(hitBlockState, blockPos);
            AxisAlignedBox hitAABB = null;
            float dist = float.MaxValue;
            foreach(AxisAlignedBox aabb in hitAABBs)
            {
                float hitDist = aabb.Intersects(this);
                if(hitDist < dist)
                {
                    dist = hitDist;
                    hitAABB = aabb;
                }
            }
            if (dist == float.MaxValue)
            {
                return null;
            }
            this.distanceToIntersection = dist;
            Vector3 exactIntersection = origin + direction * distanceToIntersection;
            Vector3 normalAtIntersection = hitAABB.GetNormalAtIntersectionPoint(exactIntersection);
            return new RayTraceResult(normalAtIntersection, exactIntersection, hitBlockState, blockPos);
        }
    }
}
