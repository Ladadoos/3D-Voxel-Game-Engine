using OpenTK;

namespace Minecraft
{
    struct BlockFace
    {
        public Vector3[] positions;
        public float[] textureCoords;
        public float[] illumination;
        public Vector3 normal;

        public BlockFace(Vector3[] positions, float[] textureCoords, float[] illumination)
        {
            this.positions = positions;
            this.textureCoords = textureCoords;
            this.illumination = illumination;
            normal = Vector3.Cross(positions[1] - positions[0], positions[2] - positions[0]);
        }
    }
}
