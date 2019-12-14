using OpenTK;

namespace Minecraft
{
    class RenderChunk
    {
        public Model hardBlocksModel { get; private set; }
        public Matrix4 transformationMatrix { get; private set; }
        public Vector2 gridPosition { get; private set; }

        public RenderChunk(Model hardBlocksModel, int gridPositionX, int gridPositionZ)
        {
            transformationMatrix = Maths.CreateTransformationMatrix(new Vector3(gridPositionX * Constants.CHUNK_SIZE, 0, gridPositionZ * Constants.CHUNK_SIZE));
            this.hardBlocksModel = hardBlocksModel;
            gridPosition = new Vector2(gridPositionX, gridPositionZ);
        }

        public void OnCloseGame()
        {
            hardBlocksModel.OnCloseGame();
        }
    }
}
