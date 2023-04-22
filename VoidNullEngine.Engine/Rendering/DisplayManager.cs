using GLFW;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering
{
    public static class DisplayManager
    {
        internal static Window Window { get; private set; }
        public static Vector2 WindowSize { get; private set; }

        internal static void CreateWindow(int width, int height, string title)
        {
            WindowSize = new Vector2(width, height);

            Glfw.Init();

            Glfw.WindowHint(Hint.ContextVersionMajor, 3);
            Glfw.WindowHint(Hint.ContextVersionMinor, 3);
            Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);

            Glfw.WindowHint(Hint.Focused, true);
            Glfw.WindowHint(Hint.Resizable, false);

            Window = Glfw.CreateWindow(width, height, title, Monitor.None, Window.None);

            if (Window == Window.None)
            {
                return;
            }

            Rectangle screen = Glfw.PrimaryMonitor.WorkArea;
            int x = (screen.Width - width) / 2;
            int y = (screen.Height - height) / 2;
            Glfw.SetWindowPosition(Window, x, y);

            Glfw.MakeContextCurrent(Window);
            Import(Glfw.GetProcAddress);

            glViewport(0, 0, width, height);
            Glfw.SwapInterval(0);
        }

        public static void RenameWindow(string title) =>
            Glfw.SetWindowTitle(Window, title);

        internal static void CloseWindow()
        {
            Glfw.Terminate();
        }
    }
}
