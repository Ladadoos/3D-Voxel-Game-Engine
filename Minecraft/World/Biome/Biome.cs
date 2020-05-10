namespace Minecraft
{
    abstract class Biome
    {
        public Block TopBlock { get; protected set; }
        public Block GradiantBlock { get; protected set; }
        public int BaseHeight { get; protected set; }
        public IDecorator Decorator { get; protected set; }
        public double Temeprature { get; protected set; }
        public double Moisture { get; protected set; }

        protected Biome()
        {
            DefineProperties();
        }

        public abstract double OffsetAt(int cx, int cy, int x, int y);

        protected abstract void DefineProperties();
    }
}
