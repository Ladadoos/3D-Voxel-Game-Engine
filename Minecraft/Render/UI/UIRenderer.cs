using OpenTK;
using System;
using System.Collections.Generic;

namespace Minecraft
{
    class UIRenderer
    {
        private readonly Dictionary<RenderSpace, List<UICanvas>> canvasses = new Dictionary<RenderSpace, List<UICanvas>>();
        private readonly CameraController cameraController;
        private readonly GameWindow window;
        private readonly UIShader uiShader;
        private readonly FontRegistry fontRegistry = new FontRegistry();
      
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
