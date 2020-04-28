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

    abstract class BlockModel
    {
        protected TextureAtlas textureAtlas;
        protected readonly BlockFace[] emptyArray = new BlockFace[0];
        protected bool back, right, front, left, top, bottom;
        public bool doubleSided { get; protected set; }

        public BlockModel(TextureAtlas textureAtlas)
        {
            this.textureAtlas = textureAtlas;
        }

        public virtual bool IsOpaqueOnSide(Direction direction)
        {
            switch(direction)
            {
                case Direction.Back: return back;
                case Direction.Right: return right;
                case Direction.Front: return front;
                case Direction.Left: return left;
                case Direction.Top: return top;
                case Direction.Bottom: return bottom;
                default: throw new System.Exception("Uncatched face.");
            }
        }

        public abstract BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos);
        public abstract BlockFace[] GetPartialVisibleFaces(BlockState state, Vector3i blockPos, Direction direction);
    }

    abstract class FullBlockModel : BlockModel
    {
        //Counter clock-wise starting bottom-left-back if facing the face from the front
        protected Vector3[] backFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
        protected Vector3[] rightFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] frontFace = new Vector3[] { new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1) };
        protected Vector3[] leftFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0) };
        protected Vector3[] topFace = new Vector3[] { new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), new Vector3(0, 1, 0) };
        protected Vector3[] bottomFace = new Vector3[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1) };

        protected float[] uvBack, uvRight, uvFront, uvLeft, uvTop, uvBottom;

        protected float[] illumination = new float[4] { 1, 1, 1, 1 };

        public FullBlockModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            SetStandardUVs();

            back = true; right = true; front = true; left = true; top = true; bottom = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return emptyArray;
        }

        public override BlockFace[] GetPartialVisibleFaces(BlockState state, Vector3i blockPos, Direction direction)
        {
            switch(direction)
            {
                case Direction.Back: return new BlockFace[] { new BlockFace(backFace, uvBack, illumination) };
                case Direction.Right: return new BlockFace[] { new BlockFace(rightFace, uvRight, illumination) };
                case Direction.Front: return new BlockFace[] { new BlockFace(frontFace, uvFront, illumination) };
                case Direction.Left: return new BlockFace[] { new BlockFace(leftFace, uvLeft, illumination) };
                case Direction.Top: return new BlockFace[] { new BlockFace(topFace, uvTop, illumination) };
                case Direction.Bottom: return new BlockFace[] { new BlockFace(bottomFace, uvBottom, illumination) };
                default: throw new System.Exception("Uncatched face.");
            }
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
            doubleSided = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return new BlockFace[] {
                new BlockFace(backFace, uvBack, illumination),
                new BlockFace(rightFace, uvRight, illumination),
                new BlockFace(frontFace, uvFront, illumination),
                new BlockFace(leftFace, uvLeft, illumination),
                new BlockFace(topFace, uvTop, illumination),
                new BlockFace(bottomFace, uvBottom, illumination)
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

    abstract class ScissorModel : BlockModel
    {
        protected Vector3[] bladeOneFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] bladeTwoFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 0) };

        protected float[] uvBladeOne, uvBladeTwo;

        protected float[] illumination = new float[4] { 1, 1, 1, 1 };

        public ScissorModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            SetStandardUVs();
            doubleSided = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return new BlockFace[] {
                new BlockFace(bladeOneFace, uvBladeOne, illumination),
                new BlockFace(bladeTwoFace, uvBladeTwo, illumination)
            };
        }

        public override BlockFace[] GetPartialVisibleFaces(BlockState state, Vector3i blockPos, Direction direction)
        {
            return emptyArray;
        }

        protected abstract void SetStandardUVs();
    }

    class BlockModelFlower : ScissorModel
    {
        public BlockModelFlower(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBladeOne = textureAtlas.GetTextureCoords(new Vector2(12, 0));
            uvBladeTwo = textureAtlas.GetTextureCoords(new Vector2(12, 0));
        }
    }

    class BlockModelSugarCane : ScissorModel
    {
        public BlockModelSugarCane(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBladeOne = textureAtlas.GetTextureCoords(new Vector2(9, 4));
            uvBladeTwo = textureAtlas.GetTextureCoords(new Vector2(9, 4));
        }
    }

    class BlockModelWheat : ScissorModel
    {
        private float[] uvBladeOneHalfMaturity;
        private float[] uvBladeTwoHalfMaturity;

        private float[] uvBladeOneFullMaturity;
        private float[] uvBladeTwoFullMaturity;

        public BlockModelWheat(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBladeOne = textureAtlas.GetTextureCoords(new Vector2(8, 5));
            uvBladeTwo = textureAtlas.GetTextureCoords(new Vector2(8, 5));

            uvBladeOneHalfMaturity = textureAtlas.GetTextureCoords(new Vector2(11, 5));
            uvBladeTwoHalfMaturity = textureAtlas.GetTextureCoords(new Vector2(11, 5));

            uvBladeOneFullMaturity = textureAtlas.GetTextureCoords(new Vector2(15, 5));
            uvBladeTwoFullMaturity = textureAtlas.GetTextureCoords(new Vector2(15, 5));
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            BlockStateWheat wheat = (BlockStateWheat)state;

            switch(wheat.maturity)
            {
                case 1:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOneHalfMaturity, illumination),
                            new BlockFace(bladeTwoFace, uvBladeTwoHalfMaturity, illumination)
                        };
                case 2:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOneFullMaturity, illumination),
                            new BlockFace(bladeTwoFace, uvBladeTwoFullMaturity, illumination)
                        };
                default:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOne, illumination),
                            new BlockFace(bladeTwoFace, uvBladeTwo, illumination)
                        };
            }
        }
    }

    class BlockModelGrassBlade : ScissorModel
    {
        private Noise3DPerlin perlin = new Noise3DPerlin(150);

        public BlockModelGrassBlade(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBladeOne = textureAtlas.GetTextureCoords(new Vector2(7, 2));
            uvBladeTwo = textureAtlas.GetTextureCoords(new Vector2(7, 2));
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            float rawOffset = (float)perlin.GetValue(blockPos.X * 0.75f, blockPos.Y * 0.75f, blockPos.Z * 0.75f);

            float offset = rawOffset / 7;
            Vector3 offsetVec = new Vector3(offset, 0, offset);

            float sizeOffset = Maths.ConvertRange(-1, 1, 0.75f, 1, rawOffset);
            Vector3 scaleVec = new Vector3(sizeOffset, sizeOffset, sizeOffset);

            Vector3[] bladeOneFaceCopy = new Vector3[bladeOneFace.Length];
            bladeOneFace.CopyTo(bladeOneFaceCopy, 0);

            Vector3[] bladeTwoFaceCopy = new Vector3[bladeTwoFace.Length];
            bladeTwoFace.CopyTo(bladeTwoFaceCopy, 0);

            for(int i = 0; i < bladeOneFaceCopy.Length; i++)
            {
                bladeOneFaceCopy[i] = bladeOneFaceCopy[i] * scaleVec + offsetVec;
            }
            for(int i = 0; i < bladeTwoFaceCopy.Length; i++)
            {
                bladeTwoFaceCopy[i] = bladeTwoFaceCopy[i] * scaleVec + offsetVec;
            }

            return new BlockFace[] {
                new BlockFace(bladeOneFaceCopy, uvBladeOne, illumination),
                new BlockFace(bladeTwoFaceCopy, uvBladeTwo, illumination)
            };
        }
    }

    class BlockModelDeadBush : ScissorModel
    {
        public BlockModelDeadBush(TextureAtlas textureAtlas) : base(textureAtlas) { }

        protected override void SetStandardUVs()
        {
            uvBladeOne = textureAtlas.GetTextureCoords(new Vector2(7, 3));
            uvBladeTwo = textureAtlas.GetTextureCoords(new Vector2(7, 3));
        }
    }
}
