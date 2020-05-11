using OpenTK;

namespace Minecraft
{
    class RayTraceResult
    {
        /// <summary> The normal of the intersection point with the block (what side it hit) </summary>
        public Vector3 NormalAtHit { get; private set; }
        /// <summary> The exact intersection point </summary>
        public Vector3 IntersectionPoint { get; private set; }
        /// <summary> The blockstate the ray hit </summary>
        public BlockState BlockstateHit { get; private set; }
        /// <summary> The position a block would be placed at if one were to be placed </summary>
        public Vector3i BlockPlacePosition { get; private set; }
        /// <summary> The grid position of the block the ray intersected </summary>
        public Vector3i IntersectedBlockPos { get; private set; }

        public RayTraceResult(Vector3 normalAtHit, Vector3 intersectedPoint, BlockState blockstateHit, Vector3i blockStatePos)
        {
            NormalAtHit = normalAtHit;
            IntersectionPoint = intersectedPoint;
            BlockstateHit = blockstateHit;
            IntersectedBlockPos = blockStatePos;
            BlockPlacePosition = blockStatePos + new Vector3i(normalAtHit, false);
        }
    }
}
