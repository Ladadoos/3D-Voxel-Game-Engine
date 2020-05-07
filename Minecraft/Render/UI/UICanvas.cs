using OpenTK;
using System.Collections.Generic;

namespace Minecraft
{
    class UICanvas
    {
        public RenderSpace renderSpace { get; protected set; }
        public int pixelWidth { get; private set; }
        public int pixelHeight { get; private set; }
        public Vector3 position { get; protected set; }
        public Vector3 rotation { get; protected set; }

        private readonly HashSet<UIComponent> components = new HashSet<UIComponent>();
        private readonly HashSet<UIComponent> toCleanComponents = new HashSet<UIComponent>();

        public UICanvas(Vector3 position, Vector3 rotation, int pixelWidth, int pixelHeight, RenderSpace renderSpace)
        {
            this.position = position;
            this.rotation = rotation;
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
            this.renderSpace = renderSpace;
        }

        public void SetDimensions(int pixelWidth, int pixelHeight)
        {
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
        }

        public void Render(UIShader uiShader)
        {
            Matrix4 transformationMatrix = Matrix4.Identity;
            if(renderSpace == RenderSpace.World)
            {
                transformationMatrix = Maths.CreateRotationAndTranslationMatrix(position, rotation);
            }
            uiShader.LoadMatrix(uiShader.location_TransformationMatrix, transformationMatrix);

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
