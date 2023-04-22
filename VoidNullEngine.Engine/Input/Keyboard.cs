using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public abstract class Keyboard
    {
        internal abstract KeyboardInput Query();

        public static string GetKeyName(Keys key) => Glfw.GetKeyName(key, 0);
    }
}
