using OpenTK;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Minecraft
{
    class UIRenderer
    {
        private Dictionary<RenderSpace, List<UICanvas>> canvasses = new Dictionary<RenderSpace, List<UICanvas>>();
        private TextureLoader textureLoader;
        private CameraController cameraController;
        private GameWindow window;
        private UIShader uiShader;
      
        public UIRenderer(GameWindow window, CameraController cameraController, TextureLoader textureLoader)
        {
            this.window = window;
            this.textureLoader = textureLoader;
            this.cameraController = cameraController;

            foreach (RenderSpace renderSpace in Enum.GetValues(typeof(RenderSpace)))
            {
                canvasses.Add(renderSpace, new List<UICanvas>());
            }

            uiShader = new UIShader();

            int fontMapTextureId = textureLoader.LoadTexture("../../Resources/arial.png");
            Texture fontTexture = new Texture(fontMapTextureId, 512, 512);
            Font font = new Font(fontTexture, "../../Resources/arial.fnt");
            UICanvas screenCanvas = new UICanvas(new Vector3(0, 100, 0), new Vector3(0, 0, 450), window.Width, window.Height, RenderSpace.World);

            UIText textComponent = new UIText(screenCanvas, font, new Vector2(360, 240), "string..O");
            UIText textComponent2 = new UIText(screenCanvas, font, new Vector2(10, 10), "voxel game");
            screenCanvas.AddComponentToRender(textComponent);
            screenCanvas.AddComponentToRender(textComponent2);
            AddCanvas(screenCanvas);
        }

        private void AddCanvas(UICanvas canvas)
        {
            if(!canvasses.TryGetValue(canvas.renderSpace, out List<UICanvas> spaceCanvasses))
            {
                Logger.Error("Failed to add canvas of type " + canvas.renderSpace);
            }
            spaceCanvasses.Add(canvas);
        }

        public void Render()
        {
            foreach (KeyValuePair<RenderSpace, List<UICanvas>> spaceCanvasses in canvasses)
            {
                foreach(UICanvas canvas in spaceCanvasses.Value)
                {
                    canvas.Clean();
                }
            }

            uiShader.Start();

            foreach(KeyValuePair<RenderSpace, List<UICanvas>> spaceCanvasses in canvasses)
            {
                if(spaceCanvasses.Key == RenderSpace.Screen)
                {
                    uiShader.LoadMatrix(uiShader.location_ViewMatrix, Matrix4.Identity);
                    uiShader.LoadMatrix(uiShader.location_ProjectionMatrix, Matrix4.Identity);
                } else
                {
                    uiShader.LoadMatrix(uiShader.location_ViewMatrix, cameraController.camera.currentViewMatrix);
                    uiShader.LoadMatrix(uiShader.location_ProjectionMatrix, cameraController.camera.currentProjectionMatrix);
                }

                spaceCanvasses.Value.ForEach(canvas => canvas.Render(uiShader));
            }
        }
    }
}
