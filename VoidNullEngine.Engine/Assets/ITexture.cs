using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Math;

namespace VoidNullEngine.Engine.Assets
{
    public interface ITexture
    {
        public uint TextureID { get; }
        public RectInt Bounds { get; }
        public Matrix3x2 Transform { get; }
        public float PixelsPerUnit { get; }
        public sealed Matrix4x4 TextureScale => Matrix4x4.CreateScale(new Vector3(Bounds.Size / PixelsPerUnit, 1));
    }

    public interface ISprite : ITexture
    {
        public ITexture SpriteSheet { get; }
    }
}
