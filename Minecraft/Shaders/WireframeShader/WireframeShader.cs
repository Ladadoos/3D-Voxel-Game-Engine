namespace Minecraft
{
    class WireframeShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/WireframeShader/vs_wireframe.glsl";
        private static readonly string fragmentFile = "../../Shaders/WireframeShader/fs_wireframe.glsl";

        public int location_TransformationMatrix;
        public int location_ViewMatrix;
        public int location_ProjectionMatrix;

        public WireframeShader() : base(vertexFile, fragmentFile)
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
