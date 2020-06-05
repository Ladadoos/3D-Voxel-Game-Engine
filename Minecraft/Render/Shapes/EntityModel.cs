using OpenTK;

namespace Minecraft
{
    abstract class EntityModel
    {
        protected readonly TextureAtlas textureAtlas;
        public BlockFace[] EntityFaces { get; protected set; }

        protected EntityModel(TextureAtlas textureAtlas)
        {
            this.textureAtlas = textureAtlas;
        }
    }

    class DummyEntityModel : EntityModel
    {
        public DummyEntityModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            Vector3[] backFace = new Vector3[] { new Vector3(0.5f, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 2f, 0), new Vector3(0.5f, 2f, 0) };
            Vector3[] rightFace = new Vector3[] { new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 2f, 0), new Vector3(0.5f, 2f, 0.5f) };
            Vector3[] frontFace = new Vector3[] { new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 2f, 0.5f), new Vector3(0, 2f, 0.5f) };
            Vector3[] leftFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 0.5f), new Vector3(0, 2f, 0.5f), new Vector3(0, 2f, 0) };
            Vector3[] topFace = new Vector3[] { new Vector3(0, 2f, 0.5f), new Vector3(0.5f, 2f, 0.5f), new Vector3(0.5f, 2f, 0), new Vector3(0, 2f, 0) };
            Vector3[] bottomFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 0.5f), new Vector3(0, 0, 0.5f) };

            float[] uvBack = textureAtlas.GetTextureCoords(new Vector2(2, 12));
            float[] uvRight = textureAtlas.GetTextureCoords(new Vector2(2, 12));
            float[] uvFront = textureAtlas.GetTextureCoords(new Vector2(2, 12));
            float[] uvLeft = textureAtlas.GetTextureCoords(new Vector2(2, 12));
            float[] uvTop = textureAtlas.GetTextureCoords(new Vector2(2, 12));
            float[] uvBottom = textureAtlas.GetTextureCoords(new Vector2(2, 12));

            EntityFaces = new BlockFace[] { new BlockFace(backFace, uvBack),
                new BlockFace(rightFace, uvRight),
                new BlockFace(frontFace, uvFront),
                new BlockFace(leftFace, uvLeft),
                new BlockFace(topFace, uvTop),
                new BlockFace(bottomFace, uvBottom) };
        }
    }
}
