namespace Minecraft
{
    class ShaderBasic : Shader
    {
        private static readonly string vertexFile = "../../Shaders/BasicShader/vertexShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/BasicShader/fragmentShader.glsl";

        public int location_TextureAtlas { get; private set; }
        public int location_TransformationMatrix { get; private set; }
        public int location_ViewMatrix { get; private set; }
        public int location_ProjectionMatrix { get; private set; }

        public ShaderBasic() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            location_TextureAtlas = GetUniformLocation("textureAtlas");
            location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            location_ViewMatrix = GetUniformLocation("viewMatrix");
            location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
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
