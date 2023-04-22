using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public readonly struct Input
    {
        public ControllerInput Controller { get; internal init; }
        public MouseInput Mouse { get; internal init; }
        public KeyboardInput Keyboard { get; internal init; }

        public Input Copy() => new Input
        {
            Controller = Controller,
            Mouse = Mouse,
            Keyboard = Keyboard
        };
    }
}
