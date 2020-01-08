using System.Collections.Generic;

namespace Minecraft
{
    class UICanvas
    {
        public bool isDirty { get; private set; }
        public RenderSpace renderSpace { get; private set; }
        public GameWindow window { get; private set; }
        private HashSet<UIComponent> components = new HashSet<UIComponent>();

        public UICanvas(GameWindow window, RenderSpace renderSpace)
        {
            this.window = window;
            this.renderSpace = renderSpace;
            isDirty = true;
        }

        public void Render(UIShader shader)
        {
            foreach(UIComponent component in components)
            {
                component.Render(shader);
            }
        }

        public bool AddComponent(UIComponent component)
        {
            if (components.Contains(component))
            {
                return false;
            }
            components.Add(component);
            return true;
        }

        public bool RemoveComponent(UIComponent component)
        {
            return components.Remove(component);
        }
    }
}
