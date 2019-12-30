using OpenTK;

namespace Minecraft
{
    abstract class EntityModel
    {
        protected TextureAtlas textureAtlas;
        public BlockFace[] entityFaces { get; protected set; }

        public EntityModel(TextureAtlas textureAtlas)
        {
            this.textureAtlas = textureAtlas;
        }
    }

    class DummyEntityModel : EntityModel
    {
        public DummyEntityModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            float[] illumination = new float[4] { 1, 1, 1, 1 };

            Vector3[] backFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
            Vector3[] rightFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) };
            Vector3[] frontFace = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
            Vector3[] leftFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) };
            Vector3[] topFace = new Vector3[] { new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) };
            Vector3[] bottomFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) };

            float[] uvBack = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            float[] uvRight = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            float[] uvFront = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            float[] uvLeft = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            float[] uvTop = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            float[] uvBottom = textureAtlas.GetTextureCoords(new Vector2(2, 0));

            entityFaces = new BlockFace[] { new BlockFace(backFace, uvBack, illumination),
                new BlockFace(rightFace, uvRight, illumination),
                new BlockFace(frontFace, uvFront, illumination),
                new BlockFace(leftFace, uvLeft, illumination),
                new BlockFace(topFace, uvTop, illumination),
                new BlockFace(bottomFace, uvBottom, illumination) };
        }
    }
}
