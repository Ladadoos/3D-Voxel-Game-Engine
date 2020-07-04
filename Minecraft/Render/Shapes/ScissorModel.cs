using OpenTK;

namespace Minecraft
{
    /// <summary>
    /// A model that consists of two quads vertically intersecting each other at a 45degree angle.
    /// </summary>
    abstract class ScissorModel : BlockModel
    {
        protected Vector3[] bladeOneFace = new Vector3[] { new Vector3(1, 0, 1), new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 1) };
        protected Vector3[] bladeTwoFace = new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 0) };

        protected Vector2[] uvBladeOne, uvBladeTwo;

        protected ScissorModel(TextureAtlas textureAtlas) : base(textureAtlas)
        {
            SetStandardUVs();
            DoubleSidedFaces = true;
        }

        public override BlockFace[] GetAlwaysVisibleFaces(BlockState state, Vector3i blockPos)
        {
            return new BlockFace[] {
                new BlockFace(bladeOneFace, uvBladeOne),
                new BlockFace(bladeTwoFace, uvBladeTwo)
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
        private Vector2[] uvBladeOneHalfMaturity;
        private Vector2[] uvBladeTwoHalfMaturity;

        private Vector2[] uvBladeOneFullMaturity;
        private Vector2[] uvBladeTwoFullMaturity;

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

            switch(wheat.Maturity)
            {
                case 1:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOneHalfMaturity),
                            new BlockFace(bladeTwoFace, uvBladeTwoHalfMaturity)
                        };
                case 2:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOneFullMaturity),
                            new BlockFace(bladeTwoFace, uvBladeTwoFullMaturity)
                        };
                default:
                    return new BlockFace[] {
                            new BlockFace(bladeOneFace, uvBladeOne),
                            new BlockFace(bladeTwoFace, uvBladeTwo)
                        };
            }
        }
    }

    class BlockModelGrassBlade : ScissorModel
    {
        private readonly Noise3DPerlin perlin = new Noise3DPerlin(150);

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
                new BlockFace(bladeOneFaceCopy, uvBladeOne),
                new BlockFace(bladeTwoFaceCopy, uvBladeTwo)
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
