using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Assets;
using VoidNullEngine.Engine.Core;
using VoidNullEngine.Engine.Rendering.Models;

namespace VoidNullEngine.Engine.Rendering.Objects
{
    public interface IElement
    {
        public GameObject GameObject { get; }
        public Matrix4x4 Transform { get; }
        public float Layer { get; set; }

        public Vector3 Priority => new(GameObject.Position, Layer);

        public Model Model { get; }
        public Shader Shader { get; set; }
        public ITexture Texture { get; set; }
    }
}
