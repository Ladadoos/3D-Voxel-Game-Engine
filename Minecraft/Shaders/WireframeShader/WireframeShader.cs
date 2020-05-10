namespace Minecraft
{
    class WireframeShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/WireframeShader/vs_wireframe.glsl";
        private static readonly string fragmentFile = "../../Shaders/WireframeShader/fs_wireframe.glsl";

        public int Location_TransformationMatrix { get; private set; }
        public int Location_ViewMatrix { get; private set; }
        public int Location_ProjectionMatrix { get; private set; }

        public WireframeShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            Location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            Location_ViewMatrix = GetUniformLocation("viewMatrix");
            Location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
        }
    }
}
