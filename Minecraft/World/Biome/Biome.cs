namespace Minecraft
{
    abstract class Biome
    {
        public Block topBlock { get; protected set; }
        public Block gradiantBlock { get; protected set; }
        public int baseHeight { get; protected set; }

        public double temperature { get; protected set; }
        public double moisture { get; protected set; }

        public Biome()
        {
            DefineProperties();
        }

        public abstract double OffsetAt(int cx, int cy, int x, int y);

        protected abstract void DefineProperties();
    }
}
