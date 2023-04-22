using GLFW;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering;

namespace VoidNullEngine.Engine.Input
{
    public sealed class LocalMouse : Mouse
    {
        private static readonly Dictionary<string, Cursor> _cursors;
        private static string _activeCursor;

        private readonly MouseCallback _callback;
        private Vector2 _scroll, _lastPosition;

        public static LocalMouse Instance { get; }

        static LocalMouse()
        {
            _cursors = new Dictionary<string, Cursor>();
            _activeCursor = null;

            Instance = new LocalMouse();
        }

        private LocalMouse()
        {
            _callback = ScrollCallback;
            _scroll = Vector2.Zero;
            _lastPosition = Position;
        }

        internal static void Initialize()
        {
            Glfw.SetScrollCallback(DisplayManager.Window, Instance._callback);
        }

        public static CursorMode Mode
        {
            get => (CursorMode)Glfw.GetInputMode(DisplayManager.Window, InputMode.Cursor);
            set => Glfw.SetInputMode(DisplayManager.Window, InputMode.Cursor, (int)value);
        }

        public static bool RawInputSupported { get; }

        public static bool UseRawInput
        {
            get => Glfw.GetInputMode(DisplayManager.Window, InputMode.RawMouseMotion) == 1;
            set
            {
                if (RawInputSupported) Glfw.SetInputMode(DisplayManager.Window, InputMode.RawMouseMotion, value ? 1 : 0);
            }
        }

        internal override MouseInput Query()
        {
            var pos = Position;
            var result = new MouseInput
            {
                IsInsideWindow = Glfw.GetWindowAttribute(DisplayManager.Window, WindowAttribute.MouseHover),
                Motion = pos - _lastPosition,
                Position = _lastPosition = pos,
                Scroll = _scroll,
                Mouse1 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button1) == InputState.Press,
                Mouse2 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button2) == InputState.Press,
                Mouse3 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button3) == InputState.Press,
                Mouse4 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button4) == InputState.Press,
                Mouse5 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button5) == InputState.Press,
                Mouse6 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button6) == InputState.Press,
                Mouse7 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button7) == InputState.Press,
                Mouse8 = Glfw.GetMouseButton(DisplayManager.Window, MouseButton.Button8) == InputState.Press
            };
            _scroll = Vector2.Zero;
            return result;
        }

        private static Vector2 Position
        {
            get
            {
                Glfw.GetCursorPosition(DisplayManager.Window, out double x, out double y);
                return new Vector2((float)x, (float)y);
            }
        }

        private void ScrollCallback(Window window, double x, double y) =>
            _scroll += new Vector2((float)x, (float)y);

        #region Custom Cursors
        public static IEnumerable<string> Cursors => _cursors.Keys;

        public static unsafe bool CreateCursor(string id, string path, int xHotspot, int yHotspot)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));
            lock (_cursors)
            {
                if (_cursors.ContainsKey(id)) return false;

                if (!File.Exists(path)) return false;

                using var image = System.Drawing.Image.FromFile(path);
                byte[] pixels;

                using (var memory = new MemoryStream())
                {
                    image.Save(memory, ImageFormat.Bmp);
                    pixels = memory.ToArray();
                }

                fixed (byte* data = pixels)
                {
                    var img = new Image(image.Width, image.Height, new IntPtr(data));
                    _cursors[id] = Glfw.CreateCursor(img, xHotspot, yHotspot);
                }

                return true;
            }
        }

        public static string ActiveCursor
        {
            get
            {
                lock (_cursors) return _activeCursor;
            }
            set
            {
                lock (_cursors)
                {
                    if (value is not null && _cursors.TryGetValue(value, out var cursor))
                    {
                        Glfw.SetCursor(DisplayManager.Window, cursor);
                        _activeCursor = value;
                        return;
                    }

                    if (_activeCursor is not null)
                    {
                        Glfw.SetCursor(DisplayManager.Window, Cursor.None);
                        _activeCursor = null;
                    }
                }
            }
        }
        #endregion
    }
}
