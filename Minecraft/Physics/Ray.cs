using OpenTK;

namespace Minecraft
{
    class Ray
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

        public RayTraceResult TraceWorld(World world)
        {
            Vector3 position = Origin;
            Vector3 offset = Direction / 50;
            BlockState hitBlockState = Blocks.Air.GetNewDefaultState();
            for (int i = 0; i < 200; i++)
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
