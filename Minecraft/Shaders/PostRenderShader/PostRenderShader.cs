namespace Minecraft
{
    class PostRenderShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/PostRenderShader/vs_postRender.glsl";
        private static readonly string fragmentFile = "../../Shaders/PostRenderShader/fs_postRender.glsl";

        public int location_colorTexture { get; private set; }
        public int location_normalDepthTexture { get; private set; }

        public PostRenderShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            location_colorTexture = GetUniformLocation("colorTexture");
            location_normalDepthTexture = GetUniformLocation("depthNormalTexture");   
        }

        protected override void BindAttributes()
        {
        }
    }
}
