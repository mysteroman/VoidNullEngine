using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public readonly struct KeyboardInput
    {
        private readonly HashSet<int> _keys;
        private readonly ModifierKeys _modifiers;
        
        internal KeyboardInput(HashSet<int> keys, ModifierKeys modifiers) => 
            (_keys, _modifiers, KeysTyped) = (keys, modifiers, string.Empty);
        public string KeysTyped { get; internal init; }
        public bool this[ModifierKeys modifiers]
        {
            get => (_modifiers & modifiers) == modifiers;
            internal init
            {
                if (value)
                {
                    _modifiers |= modifiers;
                    return;
                }
                _modifiers ^= _modifiers & modifiers;
            }
        }
        public bool this[int key]
        { 
            get => _keys is not null && _keys.Contains(key);
            internal init
            {
                if (!value && _keys is not null)
                {
                    _keys.Remove(key);
                    return;
                }
                _keys ??= new HashSet<int>();
                _keys.Add(key);
            }
        }

        public KeyboardInput Copy() => new KeyboardInput(_keys, _modifiers)
        {
            KeysTyped = KeysTyped
        };
    }
}
