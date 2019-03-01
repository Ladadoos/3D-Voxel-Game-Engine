
using OpenTK;

using Minecraft.World.Blocks;
using Minecraft.Tools;
using Minecraft.World.Sections;

namespace Minecraft.World
{
    class Chunk
    {
        public Model model;

        public Section[] sections = new Section[Constants.CHUNK_SIZE];
        public int gridX;
        public int gridZ;

        public Matrix4 transformationMatrix;

        public Chunk(int gridX, int gridZ)
        {
            this.gridX = gridX;
            this.gridZ = gridZ;

            transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridX * Constants.CHUNK_SIZE, 0, gridZ * Constants.CHUNK_SIZE), 0, 0, 0, 1, 1, 1);
        }

        public void AddBlock(int x, int y, int z, BlockType block)
        {
            int h = y / Constants.CHUNK_SIZE;
            if(sections[h] == null)
            {
                sections[h] = new Section((sbyte)h);
            }

            int localY = y - h * Constants.CHUNK_SIZE;
            sections[h].AddBlock(x, localY, z, block);
        }
    }
}
