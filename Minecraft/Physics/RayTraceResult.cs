using OpenTK;

namespace Minecraft
{
    class RayTraceResult
    {
        public AABB intersectedAABB;
        public Vector3 normalAtIntersection;
        public Vector3 intersectedPoint;
        public Vector3 intersectedGridPoint;

        public RayTraceResult(AABB intersectedAABB, Vector3 normalAtIntersection, Vector3 intersectedPoint, Vector3 intersectedGridPoint)
        {
            this.intersectedAABB = intersectedAABB;
            this.normalAtIntersection = normalAtIntersection;
            this.intersectedPoint = intersectedPoint;
            this.intersectedGridPoint = intersectedGridPoint;
        }
    }
}
