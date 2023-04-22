using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public readonly struct ControllerInput
    {
        public Vector2 LeftStick { get; internal init; }
        public Vector2 RightStick { get; internal init; }

        public float LeftTrigger { get; internal init; }
        public float RightTrigger { get; internal init; }

        public bool A { get; internal init; }
        public bool B { get; internal init; }
        public bool X { get; internal init; }
        public bool Y { get; internal init; }

        public bool LeftBumper { get; internal init; }
        public bool RightBumper { get; internal init; }

        public bool Back { get; internal init; }
        public bool Start { get; internal init; }
        public bool Guide { get; internal init; }

        public bool LeftThumb { get; internal init; }
        public bool RightThumb { get; internal init; }

        public bool Up { get; internal init; }
        public bool Right { get; internal init; }
        public bool Down { get; internal init; }
        public bool Left { get; internal init; }

        public bool Cross => A;
        public bool Circle => B;
        public bool Square => X;
        public bool Triangle => Y;

        public ControllerInput Copy() => new ControllerInput
        {
            LeftStick = LeftStick,
            RightStick = RightStick,
            LeftTrigger = LeftTrigger,
            RightTrigger = RightTrigger,
            A = A,
            B = B,
            X = X,
            Y = Y,
            LeftBumper = LeftBumper,
            RightBumper = RightBumper,
            Back = Back,
            Start = Start,
            Guide = Guide,
            LeftThumb = LeftThumb,
            RightThumb = RightThumb,
            Up = Up,
            Right = Right,
            Down = Down,
            Left = Left
        };
    }
}
