using GLFW;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public sealed class LocalController : Controller
    {
        private static readonly ConcurrentDictionary<Joystick, LocalController> _controllers;
        private static readonly JoystickCallback _callback;

        public static IEnumerable<LocalController> ActiveControllers => _controllers.Values;

        static LocalController()
        {
            _controllers = new ConcurrentDictionary<Joystick, LocalController>();
            _callback = JoystickCallback;
        }

        internal static void Initialize()
        {
            Glfw.SetJoystickCallback(_callback);
        }

        private LocalController(Joystick joystick) =>
            Joystick = joystick;

        public Joystick Joystick { get; }

        public bool IsConnected => _controllers.ContainsKey(Joystick);

        private GamePadState State => IsConnected && Glfw.GetGamepadState((int)Joystick, out var state) ? state : default;

        public string Name => IsConnected ? Glfw.GetGamepadName((int)Joystick) : null;

        internal override ControllerInput Query() =>
            new ControllerInput
            {
                LeftStick = new System.Numerics.Vector2(State.GetAxis(GamePadAxis.LeftX), State.GetAxis(GamePadAxis.LeftY)),
                RightStick = new System.Numerics.Vector2(State.GetAxis(GamePadAxis.RightX), State.GetAxis(GamePadAxis.RightY)),
                LeftTrigger = State.GetAxis(GamePadAxis.LeftTrigger),
                RightTrigger = State.GetAxis(GamePadAxis.RightTrigger),
                A = State.GetButtonState(GamePadButton.A) == InputState.Press,
                B = State.GetButtonState(GamePadButton.B) == InputState.Press,
                X = State.GetButtonState(GamePadButton.Y) == InputState.Press,
                Y = State.GetButtonState(GamePadButton.Y) == InputState.Press,
                LeftBumper = State.GetButtonState(GamePadButton.LeftBumper) == InputState.Press,
                RightBumper = State.GetButtonState(GamePadButton.RightBumper) == InputState.Press,
                Back = State.GetButtonState(GamePadButton.Back) == InputState.Press,
                Start = State.GetButtonState(GamePadButton.Start) == InputState.Press,
                Guide = State.GetButtonState(GamePadButton.Guide) == InputState.Press,
                LeftThumb = State.GetButtonState(GamePadButton.LeftThumb) == InputState.Press,
                RightThumb = State.GetButtonState(GamePadButton.RightThumb) == InputState.Press,
                Up = State.GetButtonState(GamePadButton.DpadUp) == InputState.Press,
                Down = State.GetButtonState(GamePadButton.DpadDown) == InputState.Press,
                Left = State.GetButtonState(GamePadButton.DpadLeft) == InputState.Press,
                Right = State.GetButtonState(GamePadButton.DpadRight) == InputState.Press
            };

        private static void JoystickCallback(Joystick joystick, ConnectionStatus status)
        {
            if (status == ConnectionStatus.Connected && Glfw.JoystickIsGamepad((int)joystick)) _controllers[joystick] = new LocalController(joystick);
            else _controllers.Remove(joystick, out _);
        }
    }
}
