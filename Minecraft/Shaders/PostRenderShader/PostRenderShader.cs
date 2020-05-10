namespace Minecraft
{
    class PostRenderShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/PostRenderShader/vs_postRender.glsl";
        private static readonly string fragmentFile = "../../Shaders/PostRenderShader/fs_postRender.glsl";

        public int Location_ColorTexture { get; private set; }
        public int Location_NormalDepthTexture { get; private set; }

        public PostRenderShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            Location_ColorTexture = GetUniformLocation("colorTexture");
            Location_NormalDepthTexture = GetUniformLocation("depthNormalTexture");   
        }

        protected override void BindAttributes()
        {
        }
    }
}
