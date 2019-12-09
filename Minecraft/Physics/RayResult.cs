using OpenTK;

namespace Minecraft
{
    struct RayResult
    {
        public bool intersected;
        public AABB intersectedAABB;
        public Vector3 normalAtIntersection;
        public Vector3 intersectedPoint;
        public Vector3 intersectedGridPoint;
    }
}
