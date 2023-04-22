using GLFW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering;

namespace VoidNullEngine.Engine.Input
{
    public abstract class Mouse
    {
        internal abstract MouseInput Query();
    }
}
