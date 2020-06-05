using OpenTK;

namespace Minecraft
{
    struct BlockFace
    {
        public Vector3[] Positions { get; private set; }
        public float[] TextureCoords { get; private set; }
        public Vector3 Normal { get; private set; }

        public BlockFace(Vector3[] positions, float[] textureCoords)
        {
            Positions = positions;
            TextureCoords = textureCoords;
            Normal = Vector3.Cross(positions[1] - positions[0], positions[2] - positions[0]);
        }
    }
}
