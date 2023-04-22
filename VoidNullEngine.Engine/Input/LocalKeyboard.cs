using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering;

namespace VoidNullEngine.Engine.Input
{
    public sealed class LocalKeyboard : Keyboard
    {
        private readonly HashSet<int> _keys;
        private ModifierKeys _modifiers;
        private readonly KeyCallback _keyCallback;
        private readonly StringBuilder _buffer;
        private readonly CharCallback _charCallback;

        public static LocalKeyboard Instance { get; }

        static LocalKeyboard() => Instance = new LocalKeyboard();

        internal static void Initialize()
        {
            Glfw.SetInputMode(DisplayManager.Window, InputMode.LockKeyMods, 1);
            Glfw.SetKeyCallback(DisplayManager.Window, Instance._keyCallback);
            Glfw.SetCharCallback(DisplayManager.Window, Instance._charCallback);
        }

        private LocalKeyboard()
        {
            _keys = new HashSet<int>();
            _modifiers = default;
            _keyCallback = KeyCallback;
            _buffer = new StringBuilder();
            _charCallback = CharCallback;
        }

        internal override KeyboardInput Query()
        {
            KeyboardInput result;
            lock (_buffer)
            {
                HashSet<int> keys;
                ModifierKeys modifiers;
                lock (_keys) (keys, modifiers) = (new HashSet<int>(_keys), _modifiers);
                result = new KeyboardInput(keys, modifiers)
                {
                    KeysTyped = _buffer.ToString()
                };
                _buffer.Clear();
            }
            return result;
        }

        private void CharCallback(Window window, uint codePoint)
        {
            lock (_buffer) _buffer.Append(char.ConvertFromUtf32((int)codePoint));
        }

        private void KeyCallback(Window window, Keys key, int scanCode, InputState state, ModifierKeys mods)
        {
            lock (_keys)
            {
                _modifiers = mods;
                if (state == InputState.Press) _keys.Add(scanCode);
                else _keys.Remove(scanCode);
            }
        }
    }
}
