using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Math;

namespace VoidNullEngine.Engine.Assets
{
    public sealed class Sprite : ISprite
    {
        public Sprite(ITexture spritesheet, RectInt bounds)
        {
            SpriteSheet = spritesheet ?? throw new ArgumentNullException(nameof(spritesheet));
            Vector2 trans = bounds.Origin;
            bounds.Origin += SpriteSheet.Bounds.Origin;
            if (!SpriteSheet.Bounds.Contains(bounds)) throw new ArgumentOutOfRangeException(nameof(bounds), $"The sprite's bounds aren't contained within the bounds of the parent texture");
            Bounds = bounds;
            var scale = (Vector2)Bounds.Size / SpriteSheet.Bounds.Size;
            Matrix3x2 s = Matrix3x2.CreateScale(scale);
            Matrix3x2 t = Matrix3x2.CreateTranslation(trans / SpriteSheet.Bounds.Size);
            Transform = s * t * SpriteSheet.Transform;
        }

        public ITexture SpriteSheet { get; }
        public uint TextureID => SpriteSheet.TextureID;
        public RectInt Bounds { get; }
        public Matrix3x2 Transform { get; }
        public float PixelsPerUnit => SpriteSheet.PixelsPerUnit;
    }
}
