using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Entity
{
    public struct CollisionBox
    {
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }
        public bool IsBoundingBox { get; set; }
        public CollisionFlag Flags { get; set; }
        public CollisionFlag Ignore { get; set; }

        public bool CanCollideWith(CollisionFlag flags) => Flags & (flags ^ (flags & Ignore));

        public CollisionBox Transform(Matrix3x2 transform) => new CollisionBox
        {
            TopLeft = Vector2.Transform(TopLeft, transform),
            BottomRight = Vector2.Transform(BottomRight, transform),
            IsBoundingBox = IsBoundingBox,
            Flags = Flags,
            Ignore = Ignore
        };

        public override string ToString() => (IsBoundingBox ? 'B' : 'C') +
            $"{Flags}!{Ignore}<{TopLeft.X};{TopLeft.Y};{BottomRight.X};{BottomRight.Y}>";

        public bool CollidesWith(CollisionBox box)
        {
            if (IsBoundingBox) return false;
            if (CanCollideWith(box.Flags))
            {
                if (box.IsBoundingBox)
                {
                    if (BottomRight.X > box.BottomRight.X) return true;
                    if (BottomRight.Y > box.BottomRight.Y) return true;
                    if (box.TopLeft.X > TopLeft.X) return true;
                    if (box.TopLeft.Y > TopLeft.Y) return true;
                    return false;
                }

                if (TopLeft.X > box.BottomRight.X) return false;
                if (TopLeft.Y > box.BottomRight.Y) return false;
                if (box.TopLeft.X > BottomRight.X) return false;
                if (box.TopLeft.Y > BottomRight.Y) return false;
                return true;
            }
            return false;
        }

        public bool CollidesWithAny(IEnumerable<CollisionBox> boxes)
        {
            foreach (var box in boxes)
            {
                if (CollidesWith(box)) return true;
            }
            return false;
        }
    }
}
