namespace Minecraft
{
    class ShaderBasic : Shader
    {
        private static readonly string vertexFile = "../../Shaders/BasicShader/vertexShader.glsl";
        private static readonly string fragmentFile = "../../Shaders/BasicShader/fragmentShader.glsl";

        public int location_TextureAtlas;
        public int location_TransformationMatrix;
        public int location_ViewMatrix;
        public int location_ProjectionMatrix;

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
            
        }
    }
}
