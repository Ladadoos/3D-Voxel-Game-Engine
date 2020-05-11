using OpenTK.Input;
using System.Collections.Generic;

namespace Minecraft
{
    class Input
    {
        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;

        private MouseState previousMouseState;
        private MouseState currentMouseState;

        public void Update()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetCursorState();

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
        }

        public bool OnMouseDown(MouseButton mButton)
        {
            return currentMouseState.IsButtonDown(mButton);
        }

        public bool OnMouseRelease(MouseButton mButton)
        {
            return previousMouseState.IsButtonDown(mButton) && currentMouseState.IsButtonUp(mButton);
        }

        public bool OnMousePress(MouseButton mButton)
        {
            return currentMouseState.IsButtonDown(mButton) && previousMouseState.IsButtonUp(mButton);
        }

        public bool OnKeyDown(Key key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool OnKeyRelease(Key key)
        {
            return previousKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyUp(key);
        }

        public bool OnKeyPress(Key key)
        {
            return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }

        public List<string> GetWriteKeys()
        {
            List<string> pressedKeys = new List<string>();
            foreach(Key vKey in System.Enum.GetValues(typeof(Key)))
            {
                if(vKey >= Key.A && vKey <= Key.Z)
                {
                    if(OnKeyPress(vKey))
                    {
                        pressedKeys.Add(vKey.ToString().ToLower());
                    }
                } else if(vKey == Key.Space && OnKeyPress(vKey))
                {
                    pressedKeys.Add(" ");
                }  
            }
            return pressedKeys;
        }
    }
}
