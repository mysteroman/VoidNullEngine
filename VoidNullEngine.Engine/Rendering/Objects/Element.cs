using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Assets;
using VoidNullEngine.Engine.Core;
using VoidNullEngine.Engine.Rendering.Models;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering.Objects
{
    public sealed class Element : IElement
    {
        public Element(GameObject obj, Model? model = null) => 
            (GameObject, Model) = (obj, model ?? Model.Default);
        public Element(GameObject obj, Element copy) =>
            (GameObject, Model, Layer, Shader, Texture) =
            (obj, copy.Model, copy.Layer, copy.Shader, copy.Texture);

        public GameObject GameObject { get; }
        public Matrix4x4 Transform
        {
            get
            {
                var matrix = GameObject.Transform;
                if (Texture != null)
                    matrix = Texture.TextureScale * matrix;
                return matrix;
            }
        }
        public float Layer { get; set; }

        public Model Model { get; }
        public Shader Shader { get; set; }
        public ITexture Texture { get; set; }
    }
}
