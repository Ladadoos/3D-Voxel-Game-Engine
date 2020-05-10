using OpenTK;

namespace Minecraft
{
    class RenderChunk
    {
        public VAOModel HardBlocksModel { get; set; }
        public Matrix4 TransformationMatrix { get; private set; }
        public Vector2 GridPosition { get; private set; }

        public RenderChunk(int gridPositionX, int gridPositionZ)
        {
            TransformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridPositionX * 16, 0, gridPositionZ * 16));
            GridPosition = new Vector2(gridPositionX, gridPositionZ);
        }

        public void CleanUp()
        {
            if(HardBlocksModel != null)
            {
                HardBlocksModel.CleanUp();
            }
        }
    }
}
