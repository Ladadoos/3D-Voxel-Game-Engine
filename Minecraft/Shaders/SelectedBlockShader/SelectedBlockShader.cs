namespace Minecraft
{
    class SelectedBlockShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/SelectedBlockShader/vs_selectedBlock.glsl";
        private static readonly string fragmentFile = "../../Shaders/SelectedBlockShader/fs_selectedBlock.glsl";

        public int location_TransformationMatrix;
        public int location_ViewMatrix;
        public int location_ProjectionMatrix;

        public SelectedBlockShader() : base(vertexFile, fragmentFile)
        {

        }

        protected override void GetAllUniformLocations()
        {
            location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            location_ViewMatrix = GetUniformLocation("viewMatrix");
            location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "aPos");
        }

    }
}
