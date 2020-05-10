namespace Minecraft
{
    class ShaderBasic : Shader
    {
        private static readonly string vertexFile = "../../Shaders/BasicShader/vertexShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/BasicShader/fragmentShader.glsl";

        public int Location_TextureAtlas { get; private set; }
        public int Location_TransformationMatrix { get; private set; }
        public int Location_ViewMatrix { get; private set; }
        public int Location_ProjectionMatrix { get; private set; }

        public ShaderBasic() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            Location_TextureAtlas = GetUniformLocation("textureAtlas");
            Location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            Location_ViewMatrix = GetUniformLocation("viewMatrix");
            Location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertexPosition");
            BindAttribute(1, "vertexNormal");
            BindAttribute(2, "vertexUv");
            BindAttribute(3, "vertexIllumination");
        }
    }
}
