using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    class UIRenderer
    {
        private Dictionary<RenderSpace, List<UICanvas>> canvasses = new Dictionary<RenderSpace, List<UICanvas>>();
        private CameraController cameraController;
        private GameWindow window;
        private UIShader uiShader;
        private FontRegistry fontRegistry = new FontRegistry();
      
        public UIRenderer(GameWindow window, CameraController cameraController)
        {
            this.window = window;
            this.cameraController = cameraController;
            uiShader = new UIShader();

            foreach (RenderSpace renderSpace in Enum.GetValues(typeof(RenderSpace)))
            {
                canvasses.Add(renderSpace, new List<UICanvas>());
            }

            cameraController.camera.OnProjectionChangedHandler += OnCameraProjectionChanged;

            UICanvas screenCanvas = new UICanvas(new Vector3(0, 100, 0), new Vector3(0, 0, 45), window.Width, window.Height, RenderSpace.Screen);
            UIText textComponent = new UIText(screenCanvas, fontRegistry.GetValue(FontType.Arial), new Vector2(360, 240), Vector2.One, "string..O");
            screenCanvas.AddComponentToRender(textComponent);

            AddCanvas(screenCanvas);

            UICanvas screenCanvas2 = new UICanvas(new Vector3(0, 100, 0), new Vector3(0, 0, 0), 1000, 1000, RenderSpace.World);
            UIText textComponent2 = new UIText(screenCanvas2, fontRegistry.GetValue(FontType.Arial), new Vector2(1000, 100), Vector2.One, "large text in here...!g");
            screenCanvas2.AddComponentToRender(textComponent2);

            Texture imageTexture = new Texture("../../Resources/texturePack.png", 512, 512);
            UIImage imageComponent = new UIImage(screenCanvas2, new Vector2(50, 50), new Vector2(350, 350), imageTexture);
            screenCanvas2.AddComponentToRender(imageComponent);
     
            AddCanvas(screenCanvas2);
        }

        private void OnCameraProjectionChanged(ProjectionMatrixInfo projecInfo)
        {
            if(canvasses.TryGetValue(RenderSpace.Screen, out List<UICanvas> screenCanvasses))
            {
                screenCanvasses.ForEach(canvas => canvas.SetDimensions(projecInfo.windowWidth, projecInfo.windowHeight));
            }
        }

        public Font GetFont(FontType type) => fontRegistry.GetValue(type);

        public void AddCanvas(UICanvas canvas)
        {
            if(!canvasses.TryGetValue(canvas.renderSpace, out List<UICanvas> spaceCanvasses))
            {
                Logger.Error("Failed to add canvas of type " + canvas.renderSpace);
            }
            spaceCanvasses.Add(canvas);
        }

        public void RemoveCanvas(UICanvas canvas)
        {
            if (!canvasses.TryGetValue(canvas.renderSpace, out List<UICanvas> spaceCanvasses))
            {
                Logger.Error("Failed to remove canvas of type " + canvas.renderSpace);
            }
            spaceCanvasses.Remove(canvas);
        }

        public void Render()
        {
            foreach (KeyValuePair<RenderSpace, List<UICanvas>> spaceCanvasses in canvasses)
            {
                foreach(UICanvas canvas in spaceCanvasses.Value)
                {
                    canvas.Update();
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
