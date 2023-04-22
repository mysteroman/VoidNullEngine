using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Math
{
    public static class Vector2Extensions
    {
        public static void Deconstruct(this Vector2 v, out float x, out float y) => (x, y) = (v.X, v.Y);
    }
}
