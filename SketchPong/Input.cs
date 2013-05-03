using Microsoft.Xna.Framework.Input;

namespace sketchPong
{
    public class Input
    {
        private KeyboardState keyboardState;
        private KeyboardState lastState;

        public Input()
        {
            keyboardState = Keyboard.GetState();
            lastState = keyboardState;
        }

        public void Update()
        {
            lastState = keyboardState;
            keyboardState = Keyboard.GetState();
        }

        public bool PressStart
        {
            get
            {
                return keyboardState.IsKeyDown(Keys.Enter) && lastState.IsKeyUp(Keys.Enter);
            }
        }

        public bool PressSpace
        {            get{
            return keyboardState.IsKeyDown(Keys.Space) && lastState.IsKeyUp(Keys.Space);
        }
        }

    }
}