using OpenTK;

namespace Minecraft
{
    class RayTraceResult
    {
        /// <summary> The normal of the intersection point with the block (what side it hit) </summary>
        public Vector3 normalAtHit { get; private set; }
        /// <summary> The exact intersection point </summary>
        public Vector3 intersectedPoint { get; private set; }
        /// <summary> The blockstate the ray hit </summary>
        public BlockState blockstateHit { get; private set; }
        /// <summary> The position a block would be placed at if one were to be placed </summary>
        public Vector3i blockPlacePosition { get; private set; }
        /// <summary> The grid position of the block the ray intersected </summary>
        public Vector3i intersectedBlockPos { get; private set; }

        public RayTraceResult(Vector3 normalAtHit, Vector3 intersectedPoint, BlockState blockstateHit, Vector3i blockStatePos)
        {
            this.normalAtHit = normalAtHit;
            this.intersectedPoint = intersectedPoint;
            this.blockstateHit = blockstateHit;
            this.intersectedBlockPos = blockStatePos;
            this.blockPlacePosition = blockStatePos + new Vector3i(normalAtHit, false);
        }
    }
}
