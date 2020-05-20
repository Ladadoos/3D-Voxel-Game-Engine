namespace Minecraft
{
    class SkydomeShader : Shader
    {
        private static readonly string vertexFile = "../../Shaders/Skydome/vs_skydome.glsl";
        private static readonly string fragmentFile = "../../Shaders/Skydome/fs_skydome.glsl";

        public int Location_ProjectionMatrix { get; private set; }
        public int Location_SunPosition { get; private set; }
        public int Location_CurrentTime { get; private set; }
        public int Location_TopSkyColor { get; private set; } 
        public int Location_BottomSkyColor { get; private set; }
        public int Location_HorizonColor { get; private set; }
        public int Location_SunColor { get; private set; }
        public int Location_SunGlowColor { get; private set; }
        public int Location_MoonColor { get; private set; }
        public int Location_MoonGlowColor { get; private set; }
        public int Location_DitherTexture { get; private set; }

        public SkydomeShader() : base(vertexFile, fragmentFile)
        {
        }

        protected override void GetAllUniformLocations()
        {
            Location_ProjectionMatrix = GetUniformLocation("viewProjectionMatrix");
            Location_SunPosition = GetUniformLocation("sunPosition");
            Location_CurrentTime = GetUniformLocation("time");
            Location_TopSkyColor = GetUniformLocation("topSkyColor");
            Location_BottomSkyColor = GetUniformLocation("bottomSkyColor");
            Location_HorizonColor = GetUniformLocation("horizonColor");
            Location_SunColor = GetUniformLocation("sunColor");
            Location_SunGlowColor = GetUniformLocation("sunGlowColor");
            Location_MoonColor = GetUniformLocation("moonColor");
            Location_MoonGlowColor = GetUniformLocation("moonGlowColor");
            Location_DitherTexture = GetUniformLocation("ditherTexture");
        }

        protected override void BindAttributes()
        {
            BindAttribute(0, "vertexPosition");
        }
    }
}
