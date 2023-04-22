using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Math
{
    public struct Rect : IEquatable<Rect>
    {
        private Vector2 min, max;

        public Rect(float x, float y, float width, float height)
        {
            if (!float.IsFinite(x)) throw new ArgumentOutOfRangeException(nameof(x), $"Value out of range: {x}");
            if (!float.IsFinite(y)) throw new ArgumentOutOfRangeException(nameof(y), $"Value out of range: {y}");
            if (!float.IsFinite(width) || width < 0) throw new ArgumentOutOfRangeException(nameof(width), $"Value out of range: {width}");
            if (!float.IsFinite(height) || height < 0) throw new ArgumentOutOfRangeException(nameof(height), $"Value out of range: {height}");
            min = new(x, y);
            max = new(x + width, y + height);
        }

        public Rect(Vector2 min, Vector2 max)
        {
            if (!float.IsFinite(min.X) || !float.IsFinite(min.Y)) throw new ArgumentOutOfRangeException(nameof(min), $"Value out of range: {min}");
            if (!float.IsFinite(max.X) || !float.IsFinite(max.Y) || max.X < min.X || max.Y < min.Y) throw new ArgumentOutOfRangeException(nameof(max), $"Value out of range: {max}");
            this.min = min;
            this.max = max;
        }

        public Rect(Vector2 size)
        {
            if (!float.IsFinite(size.X) || !float.IsFinite(size.Y) || size.X < 0 || size.Y < 0) throw new ArgumentOutOfRangeException(nameof(size), $"Value out of range: {size}");
            min = Vector2.Zero;
            max = size;
        }

        public static Rect FromPoints(params Vector2[] points) => FromPoints(collection: points);

        public static Rect FromPoints(IEnumerable<Vector2> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2 min = Vector2.Zero, max = Vector2.Zero;
            foreach (var point in collection)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) continue;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return new Rect(min, max);
        }

        public static Rect Union(Rect left, Rect right) =>
            new(Vector2.Min(left.min, right.min), Vector2.Max(left.max, right.max));

        public static Rect Union(Rect left, Vector2 right)
        {
            if (!float.IsFinite(right.X) || !float.IsFinite(right.Y)) throw new ArgumentOutOfRangeException(nameof(right), $"Vector isn't finite: {right}");
            return new(Vector2.Min(left.min, right), Vector2.Max(left.max, right));
        }

        public static Rect UnionAll(Rect left, params Vector2[] right) => UnionAll(left, collection: right);
        public static Rect UnionAll(Rect left, IEnumerable<Vector2> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2 min = left.min, max = left.max;
            foreach (var point in collection)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) continue;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            return new Rect(min, max);
        }

        public static Rect Intersection(Rect left, Rect right)
        {
            Vector2 min = Vector2.Max(left.min, right.min);
            Vector2 max = Vector2.Min(left.max, right.max);
            if (min.X > max.X || min.Y > max.Y) return default;
            return new(min, max);
        }

        public readonly bool Contains(Vector2 point)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) return false;
            return point.X >= min.X && point.Y >= min.Y && point.X <= max.X && point.Y <= max.Y;
        }

        public readonly bool Contains(Rect rect) =>
            min.X <= rect.min.X && min.Y <= rect.min.Y && max.X >= rect.max.X && max.Y >= rect.max.Y;

        public readonly bool OverlapsWith(Rect rect)
        {
            int minX = min.X >= rect.min.X ? min.X <= rect.max.X ? 0 : 1 : -1;
            int minY = min.Y >= rect.min.Y ? min.Y <= rect.max.Y ? 0 : 1 : -1;
            
            int maxX = max.X >= rect.min.X ? max.X <= rect.max.X ? 0 : 1 : -1;
            int maxY = max.Y >= rect.min.Y ? max.Y <= rect.max.Y ? 0 : 1 : -1;

            return (minX * maxX) < 1 && (minY * maxY) < 1;
        }

        public void UnionWith(Rect rect)
        {
            min = Vector2.Min(min, rect.min);
            max = Vector2.Max(max, rect.max);
        }

        public void UnionWith(Vector2 point)
        {
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentOutOfRangeException(nameof(point), $"Vector isn't finite: {point}");
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }

        public void UnionWithAll(params Vector2[] points) => UnionWithAll(collection: points);

        public void UnionWithAll(IEnumerable<Vector2> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2 min = this.min, max = this.max;
            foreach (var point in collection)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) continue;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            this.min = min;
            this.max = max;
        }

        public void IntersectWith(Rect rect)
        {
            Vector2 min = Vector2.Max(this.min, rect.min);
            Vector2 max = Vector2.Min(this.max, rect.max);
            if (min.X > max.X || min.Y > max.Y)
            {
                this.min = this.max = Vector2.Zero;
                return;
            }
            (this.min, this.max) = (min, max);
        }

        public readonly bool Equals(Rect other) =>
            min == other.min && max == other.max;

        public readonly override bool Equals(object obj) =>
            obj is Rect rect && Equals(rect);

        public readonly override int GetHashCode() => HashCode.Combine(typeof(Rect), min, max);

        public readonly override string ToString() =>
            $"Rect({min.X}, {min.Y}; {max.X}, {max.Y})";

        public float Width
        {
            readonly get => max.X - min.X;
            set
            {
                if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.X = min.X + value;
            }
        }

        public float Height
        {
            readonly get => max.Y - min.Y;
            set
            {
                if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.Y = min.Y + value;
            }
        }

        public Vector2 Size
        {
            readonly get => new(Width, Height);
            set => (Width, Height) = value;
        }

        public float X
        {
            readonly get => min.X;
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                float w = Width;
                min.X = value;
                max.X = min.X + w;
            }
        }

        public float Y
        {
            readonly get => min.Y;
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                float h = Height;
                min.Y = value;
                max.Y = min.Y + h;
            }
        }

        public Vector2 Origin
        {
            readonly get => min;
            set
            {
                if (!float.IsFinite(value.X)) throw new ArgumentOutOfRangeException(nameof(value.X), $"Value out of range: {value.X}");
                if (!float.IsFinite(value.Y)) throw new ArgumentOutOfRangeException(nameof(value.Y), $"Value out of range: {value.Y}");
                var s = Size;
                min = value;
                max = value + s;
            }
        }

        public float MinX
        {
            readonly get => min.X;
            set
            {
                if (!float.IsFinite(value) || value > max.X) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                min.X = value;
            }
        }

        public float MinY
        {
            readonly get => min.Y;
            set
            {
                if (!float.IsFinite(value) || value > max.Y) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                min.Y = value;
            }
        }

        public Vector2 Min
        {
            readonly get => min;
            set
            {
                if (!float.IsFinite(value.X) || value.X > max.X) throw new ArgumentOutOfRangeException(nameof(value.X), $"Value out of range: {value.X}");
                if (!float.IsFinite(value.Y) || value.Y > max.Y) throw new ArgumentOutOfRangeException(nameof(value.Y), $"Value out of range: {value.Y}");
                min = value;
            }
        }

        public float MaxX
        {
            readonly get => max.X;
            set
            {
                if (!float.IsFinite(value) || value < min.X) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.X = value;
            }
        }

        public float MaxY
        {
            readonly get => max.Y;
            set
            {
                if (!float.IsFinite(value) || value < min.Y) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.Y = value;
            }
        }

        public Vector2 Max
        {
            readonly get => max;
            set
            {
                if (!float.IsFinite(value.X) || value.X < min.X) throw new ArgumentOutOfRangeException(nameof(value.X), $"Value out of range: {value.X}");
                if (!float.IsFinite(value.Y) || value.Y < min.Y) throw new ArgumentOutOfRangeException(nameof(value.Y), $"Value out of range: {value.Y}");
                max = value;
            }
        }

        public static Rect operator |(Rect left, Rect right) => Union(left, right);
        public static Rect operator |(Rect left, Vector2 right) => Union(left, right);
        public static Rect operator |(Vector2 left, Rect right) => Union(right, left);
        public static Rect operator &(Rect left, Rect right) => Intersection(left, right);

        public static bool operator ==(Rect left, Rect right) => left.Equals(right);
        public static bool operator !=(Rect left, Rect right) => !(left == right);
    }
}
