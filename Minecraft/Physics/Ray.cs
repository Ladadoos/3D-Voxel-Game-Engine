using OpenTK;

namespace Minecraft
{
    struct Ray
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
    }
}
