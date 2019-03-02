using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /*
         * Mouse
         */
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

        /*
         * Keyboard
         */
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
    }
}
