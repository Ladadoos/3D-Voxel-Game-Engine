namespace Minecraft
{
    class UIShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/UIShader/vs_uiShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/UIShader/fs_uiShader.glsl";

        public int Location_Texture { get; private set; }
        public int Location_TransformationMatrix { get; private set; }
        public int Location_ViewMatrix { get; private set; }
        public int Location_ProjectionMatrix { get; private set; }

        public UIShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            Location_Texture = GetUniformLocation("uiTexture");
            Location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            Location_ViewMatrix = GetUniformLocation("viewMatrix");
            Location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertexPosition");
            BindAttribute(1, "vertexUv");
        }
    }
}
