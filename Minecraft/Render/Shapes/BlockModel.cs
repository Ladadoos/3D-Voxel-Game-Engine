namespace Minecraft
{
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
}
