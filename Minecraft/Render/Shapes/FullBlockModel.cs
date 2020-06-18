using OpenTK;

namespace Minecraft
{
    abstract class FullBlockModel : BlockModel
    {
        //Counter clock-wise starting bottom-left-back if facing the face from the front
        protected Vector3[] backFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        protected Vector3[] rightFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] frontFace = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
        protected Vector3[] leftFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) };
        protected Vector3[] topFace = new Vector3[] { new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0) };
        protected Vector3[] bottomFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0) };

        protected Vector2[] uvBack, uvRight, uvFront, uvLeft, uvTop, uvBottom;

        protected FullBlockModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            SetStandardUVs();

            back = true; right = true; front = true; left = true; top = true; bottom = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return emptyArray;
        }

        private static BlockFace[] partialFaces = new BlockFace[1];
        public override BlockFace[] GetPartialVisibleFaces(BlockState state, Vector3i blockPos, Direction direction)
        {
            switch(direction)
            {
                case Direction.Back: partialFaces[0] = new BlockFace(backFace, uvBack); break;
                case Direction.Right: partialFaces[0] = new BlockFace(rightFace, uvRight); break;
                case Direction.Front:  partialFaces[0] = new BlockFace(frontFace, uvFront); break;
                case Direction.Left: partialFaces[0] = new BlockFace(leftFace, uvLeft); break;
                case Direction.Top: partialFaces[0] = new BlockFace(topFace, uvTop); break;
                case Direction.Bottom: partialFaces[0] = new BlockFace(bottomFace, uvBottom); break;
                default: throw new System.Exception("Uncatched face.");
            }
             return partialFaces;
        }

        protected abstract void SetStandardUVs();
    }

    class BlockModelDirt : FullBlockModel
    {
        public BlockModelDirt(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(2, 0));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(2, 0));
        }
    }

    class BlockModelStone : FullBlockModel
    {
        public BlockModelStone(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(1, 0));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(1, 0));
        }
    }

    class BlockModelSand : FullBlockModel
    {
        public BlockModelSand(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(2, 1));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(2, 1));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(2, 1));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(2, 1));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(2, 1));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(2, 1));
        }
    }

    class BlockModelTNT : FullBlockModel
    {
        public BlockModelTNT(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(8, 0));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(8, 0));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(8, 0));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(8, 0));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(9, 0));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(10, 0));
        }
    }

    class BlockModelGrass : FullBlockModel
    {
        public BlockModelGrass(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(3, 0));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(3, 0));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(3, 0));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(3, 0));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(0, 0));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(2, 0));
        }
    }

    class BlockModelSandstone : FullBlockModel
    {
        public BlockModelSandstone(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(0, 12));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(0, 12));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(0, 12));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(0, 12));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(0, 11));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(0, 13));
        }
    }

    class BlockModelOakLog : FullBlockModel
    {
        public BlockModelOakLog(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(4, 1));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(4, 1));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(4, 1));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(4, 1));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(5, 1));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(5, 1));
        }
    }

    class BlockModelOakLeaves : FullBlockModel
    {
        public BlockModelOakLeaves(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(5, 3));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(5, 3));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(5, 3));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(5, 3));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(5, 3));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(5, 3));
        }
    }

    class BlockModelGravel : FullBlockModel
    {
        public BlockModelGravel(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(3, 1));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(3, 1));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(3, 1));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(3, 1));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(3, 1));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(3, 1));
        }
    }

    class BlockModelCactus : FullBlockModel
    {
        public BlockModelCactus(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            float dt = 0.0625f;
            float dtx = 0.9375f;
            backFace = new Vector3[] { new Vector3(1, 0, dt), new Vector3(0, 0, dt), new Vector3(0, 1, dt), new Vector3(1, 1, dt) };
            rightFace = new Vector3[] { new Vector3(dtx, 0, 1), new Vector3(dtx, 0, 0), new Vector3(dtx, 1, 0), new Vector3(dtx, 1, 1) };
            frontFace = new Vector3[] { new Vector3(0, 0, dtx), new Vector3(1, 0, dtx), new Vector3(1, 1, dtx), new Vector3(0, 1, dtx) };
            leftFace = new Vector3[] { new Vector3(dt, 0, 0), new Vector3(dt, 0, 1), new Vector3(dt, 1, 1), new Vector3(dt, 1, 0) };

            back = false; right = false; front = false; left = false; top = false; bottom = false;
            DoubleSidedFaces = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return new BlockFace[] {
                new BlockFace(backFace, uvBack),
                new BlockFace(rightFace, uvRight),
                new BlockFace(frontFace, uvFront),
                new BlockFace(leftFace, uvLeft),
                new BlockFace(topFace, uvTop),
                new BlockFace(bottomFace, uvBottom)
            };
        }

        public override BlockFace[] GetPartialVisibleFaces(BlockState state, Vector3i blockPos, Direction direction)
        {
            return emptyArray;
        }

        protected override void SetStandardUVs()
        {
            uvBack = textureAtlas.GetTextureCoords(new Vector2(6, 4));
            uvRight = textureAtlas.GetTextureCoords(new Vector2(6, 4));
            uvFront = textureAtlas.GetTextureCoords(new Vector2(6, 4));
            uvLeft = textureAtlas.GetTextureCoords(new Vector2(6, 4));
            uvTop = textureAtlas.GetTextureCoords(new Vector2(5, 4));
            uvBottom = textureAtlas.GetTextureCoords(new Vector2(7, 4));
        }
    }
}
