using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Objects
{
    public struct DirectionalLight
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float Focus { get; set; }

        public Vector3 AmbientLight { get; set; }
        public Vector4 Light { get; set; }
    }
}
