namespace Minecraft
{
    class ShaderBasic : Shader
    {
        private static readonly string vertexFile = "../../Shaders/GLSL/vertexShader.txt";
        private static readonly string fragmentFile = "../../Shaders/GLSL/fragmentShader.txt";

        public int location_Texture1;
        public int location_TransformationMatrix;
        public int location_ViewMatrix;
        public int location_ProjectionMatrix;

        public ShaderBasic() : base(vertexFile, fragmentFile)
        {

        }

        protected override void GetAllUniformLocations()
        {
            location_Texture1 = GetUniformLocation("texture1");
            location_TransformationMatrix = GetUniformLocation("transformationMatrix");
            location_ViewMatrix = GetUniformLocation("viewMatrix");
            location_ProjectionMatrix = GetUniformLocation("projectionMatrix");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "aPos");
            BindAttribute(1, "aTexCoord");
        }

    }
}
