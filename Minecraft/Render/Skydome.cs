using OpenTK.Graphics.OpenGL;

namespace Minecraft
{
    class Skydome
    {
        private SkydomeShader skydomeShader = new SkydomeShader();
        private VAOModel skydomeModel;
        private Game game;

        public Skydome(Game game)
        {
            this.game = game;

            ModelData model = OBJLoader.Load("../../Resources/sphere.obj");
            skydomeModel = new VAOModel(model.positions, model.indices);
        }

        public void Render()
        {
            GL.DepthMask(false);
            GL.Disable(EnableCap.CullFace);
            skydomeShader.Start();

            Camera activeCamera = game.MasterRenderer.GetActiveCamera();
            skydomeShader.LoadMatrix(skydomeShader.Location_ProjectionMatrix, activeCamera.CurrentViewMatrix.ClearTranslation() * activeCamera.CurrentProjectionMatrix);

            Environment environment = game.World.Environment;
            skydomeShader.LoadInt(skydomeShader.Location_CurrentTime, environment.CurrentTime);
            skydomeShader.LoadVector(skydomeShader.Location_SunPosition, environment.SunPosition);
            skydomeShader.LoadVector(skydomeShader.Location_TopSkyColor, environment.GetCurrentTopSkyColor());
            skydomeShader.LoadVector(skydomeShader.Location_BottomSkyColor, environment.GetCurrentBottomSkyColor());
            skydomeShader.LoadVector(skydomeShader.Location_HorizonColor, environment.GetCurrentHorizonColor());
            skydomeShader.LoadVector(skydomeShader.Location_SunColor, environment.GetCurrentSunColor());
            skydomeShader.LoadVector(skydomeShader.Location_SunGlowColor, environment.GetCurrentSunGlowColor());
            skydomeShader.LoadVector(skydomeShader.Location_MoonColor, environment.GetCurrentMoonColor());
            skydomeShader.LoadVector(skydomeShader.Location_MoonGlowColor, environment.GetCurrentMoonGlowColor());

            skydomeShader.LoadTexture(skydomeShader.Location_DitherTexture, 0, game.MasterRenderer.DitherTextureId);

            skydomeModel.BindVAO();
            GL.DrawElements(PrimitiveType.Triangles, skydomeModel.IndicesCount, DrawElementsType.UnsignedInt, 0);
            skydomeModel.UnbindVAO();
            skydomeShader.Stop();

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthMask(true);
        }
    }
}
