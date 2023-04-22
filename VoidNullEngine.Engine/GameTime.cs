using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFW;

namespace VoidNullEngine.Engine
{
    static class GameTime
    {
        public static float DeltaTime { get; private set; }
        public static float TotalElapsedSeconds { get; private set; }

        public static void Tick()
        {
            DeltaTime = (float)Glfw.Time - TotalElapsedSeconds;
            TotalElapsedSeconds = (float)Glfw.Time;
        }
    }
}
