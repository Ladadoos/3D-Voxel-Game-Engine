using Minecraft.Tools;
using Minecraft.World;
using Minecraft.World.Blocks;
using OpenTK;

namespace Minecraft
{
    class RenderChunk
    {
        public Model HardBlocksModel {get; private set; }
        public Matrix4 TransformationMatrix { get; private set; }

        public RenderChunk(Model hardBlocksModel, int gridPositionX, int gridPositionY)
        {
            TransformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridPositionX * Constants.CHUNK_SIZE, 0, gridPositionY * Constants.CHUNK_SIZE));
            HardBlocksModel = hardBlocksModel;
        }

        public void OnApplicationClosed()
        {
            HardBlocksModel.CleanUp();
        }
    }
}
