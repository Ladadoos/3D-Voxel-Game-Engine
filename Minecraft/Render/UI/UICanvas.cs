using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class UICanvas
    {
        public RenderSpace RenderSpace { get; protected set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        public Vector3 Position { get; protected set; }
        public Vector3 Rotation { get; protected set; }

        private readonly HashSet<UIComponent> components = new HashSet<UIComponent>();
        private readonly HashSet<UIComponent> toCleanComponents = new HashSet<UIComponent>();

        public UICanvas(Vector3 position, Vector3 rotation, int pixelWidth, int pixelHeight, RenderSpace renderSpace)
        {
            Position = position;
            Rotation = rotation;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            RenderSpace = renderSpace;
        }

        public void SetDimensions(int pixelWidth, int pixelHeight)
        {
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
        }

        public void Render(UIShader uiShader)
        {
            Matrix4 transformationMatrix = Matrix4.Identity;
            if(RenderSpace == RenderSpace.World)
            {
                transformationMatrix = Maths.CreateRotationAndTranslationMatrix(Position, Rotation);
            }
            uiShader.LoadMatrix(uiShader.Location_TransformationMatrix, transformationMatrix);

            foreach (UIComponent component in components)
            {
                component.Render(uiShader);
            }
        }

        public void Clean()
        {
            foreach(UIComponent toCleanComp in toCleanComponents)
            {
                toCleanComp.Clean();
            }
            toCleanComponents.Clear();
        }

        public void AddComponentToClean(UIComponent component)
        {
            if (!toCleanComponents.Contains(component))
            {
                toCleanComponents.Add(component);
            }
        }

        public bool AddComponentToRender(UIComponent component)
        {
            if (components.Contains(component))
            {
                return false;
            }
            components.Add(component);
            return true;
        }

        public bool RemoveComponentFromRender(UIComponent component)
        {
            return components.Remove(component);
        }

        public virtual void Update() { }
    }
}
