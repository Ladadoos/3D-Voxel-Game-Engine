namespace Minecraft
{
    class UIShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/UIShader/vs_uiShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/UIShader/fs_uiShader.glsl";

        public int location_Texture { get; private set; }
        public int location_TransformationMatrix { get; private set; }
        public int location_ViewMatrix { get; private set; }
        public int location_ProjectionMatrix { get; private set; }

        public UIShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            location_Texture = GetUniformLocation("uiTexture");
            location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            location_ViewMatrix = GetUniformLocation("viewMatrix");
            location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertexPosition");
            BindAttribute(1, "vertexUv");
        }
    }
}
