using OpenTK;

namespace Minecraft
{
    struct Ray
    {
        public Vector3 Origin { get; private set; }
        public Vector3 Direction { get; private set; }
        public Vector3 DirectionFrac { get; private set; }
        public float DistanceToIntersection { get; private set; }

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction.Normalized();
            DirectionFrac = new Vector3(1 / Direction.X, 1 / Direction.Y, 1 / Direction.Z);
            DistanceToIntersection = float.MaxValue;
        }

        public RayTraceResult TraceWorld(World world, int maxDist = 1, int stepsPerBlock = 50)
        {
            Vector3 originalPosition = Origin;
            Vector3 position = Origin;
            int maxSteps = maxDist * stepsPerBlock;
            Vector3 offset = Direction / stepsPerBlock;
            BlockState hitBlockState = Blocks.GetState(Blocks.Air);
            for (int i = 0; i < maxSteps; i++)
            {
                position += offset;
                hitBlockState = world.GetBlockAt(new Vector3i(position));
                if (hitBlockState.GetBlock() != Blocks.Air)
                {
                    break;
                }
            }

            if (hitBlockState.GetBlock() == Blocks.Air)
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
            if (dist == float.MaxValue || hitAABB == null)
            {
                return null;
            }
            DistanceToIntersection = dist;
            Vector3 exactIntersection = Origin + Direction * DistanceToIntersection;
            Vector3 normalAtIntersection = hitAABB.GetNormalAtIntersectionPoint(exactIntersection);
            return new RayTraceResult(normalAtIntersection, exactIntersection, hitBlockState, blockPos);
        }
    }
}
