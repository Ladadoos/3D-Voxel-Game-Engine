using OpenTK;

namespace Minecraft
{
    class RayTraceResult
    {
        public AABB aabbHit { get; private set; }
        public Vector3 normalAtHit { get; private set; }
        public Vector3 intersectedPoint { get; private set; }
        public BlockState blockstateHit { get; private set; }
        public Vector3 blockPlacePosition { get; private set; }

        public RayTraceResult(AABB aabbHit, Vector3 normalAtHit, Vector3 intersectedPoint, BlockState blockstateHit)
        {
            this.aabbHit = aabbHit;
            this.normalAtHit = normalAtHit;
            this.intersectedPoint = intersectedPoint;
            this.blockstateHit = blockstateHit;
            this.blockPlacePosition = blockstateHit.position + normalAtHit;
        }
    }
}
