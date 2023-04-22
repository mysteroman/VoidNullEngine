using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public readonly struct MouseInput
    {
        public bool IsInsideWindow { get; internal init; }

        public Vector2 Position { get; internal init; }
        public Vector2 Motion { get; internal init; }
        public Vector2 Scroll { get; internal init; }

        public bool Mouse1 { get; internal init; }
        public bool Mouse2 { get; internal init; }
        public bool Mouse3 { get; internal init; }
        public bool Mouse4 { get; internal init; }
        public bool Mouse5 { get; internal init; }
        public bool Mouse6 { get; internal init; }
        public bool Mouse7 { get; internal init; }
        public bool Mouse8 { get; internal init; }

        public bool Left => Mouse1;
        public bool Right => Mouse2;
        public bool Middle => Mouse3;

        public MouseInput Copy() => new MouseInput
        {
            IsInsideWindow = IsInsideWindow,
            Position = Position,
            Motion = Motion,
            Scroll = Scroll,
            Mouse1 = Mouse1,
            Mouse2 = Mouse2,
            Mouse3 = Mouse3,
            Mouse4 = Mouse4,
            Mouse5 = Mouse5,
            Mouse6 = Mouse6,
            Mouse7 = Mouse7,
            Mouse8 = Mouse8
        };
    }
}
