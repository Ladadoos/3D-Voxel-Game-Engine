using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class UIRenderer
    {
        public List<UICanvas> canvasses = new List<UICanvas>();

        private TextureLoader textureLoader;
        private CameraController cameraController;
        private GameWindow window;
        private UIShader uiShader;

        public UIRenderer(GameWindow window, CameraController cameraController, TextureLoader textureLoader)
        {
            this.window = window;
            this.textureLoader = textureLoader;
            this.cameraController = cameraController;

            uiShader = new UIShader();

            int fontMapTextureId = textureLoader.LoadTexture("../../Resources/fontMap.png");
            Texture fontTexture = new Texture(fontMapTextureId, 766, 16);
            Font font = new Font(fontTexture, 8);
            UICanvas screenCanvas = new UICanvas(window, RenderSpace.Screen);

            UIText textComponent = new UIText(screenCanvas, font, new Vector2(0.2f, 0.2f), "dion");
            screenCanvas.AddComponent(textComponent);
            canvasses.Add(screenCanvas);
        }

        public void Render()
        {
            uiShader.Start();
            uiShader.LoadMatrix(uiShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);
            canvasses.ForEach(canvas => canvas.Render(uiShader));
        }
    }
}
