using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Math
{
    public struct RectInt : IEquatable<RectInt>, IEnumerable<Vector2Int>
    {
        private Vector2Int min, max;

        public RectInt(int x, int y, int width, int height)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width), $"Value out of range: {width}");
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height), $"Value out of range: {height}");
            min = new(x, y);
            max = checked(new(x + width, y + height));
        }

        public RectInt(Vector2Int min, Vector2Int max)
        {
            if (max.X < min.X || max.Y < min.Y) throw new ArgumentOutOfRangeException(nameof(max), $"Value out of range: {max}");
            this.min = min;
            this.max = max;
        }

        public RectInt(Vector2Int size)
        {
            if (size.X < 0 || size.Y < 0) throw new ArgumentOutOfRangeException(nameof(size), $"Value out of range: {size}");
            min = Vector2Int.Zero;
            max = size;
        }

        public static RectInt FromPoints(params Vector2Int[] points) => FromPoints(collection: points);

        public static RectInt FromPoints(IEnumerable<Vector2Int> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2Int min = Vector2Int.Zero, max = Vector2Int.Zero;
            foreach (var point in collection)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) continue;
                min = Vector2Int.Min(min, point);
                max = Vector2Int.Max(max, point + Vector2Int.One);
            }
            return new(min, max);
        }

        public static RectInt Union(RectInt left, RectInt right) =>
            new(Vector2Int.Min(left.min, right.min), Vector2Int.Max(left.max, right.max));

        public static RectInt Union(RectInt left, Vector2Int right) =>
            new(Vector2Int.Min(left.min, right), Vector2Int.Max(left.max, right + Vector2Int.One));

        public static RectInt UnionAll(RectInt left, params Vector2Int[] right) => UnionAll(left, collection: right);
        public static RectInt UnionAll(RectInt left, IEnumerable<Vector2Int> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2Int min = left.min, max = left.max;
            foreach (var point in collection)
            {
                min = Vector2Int.Min(min, point);
                max = Vector2Int.Max(max, point + Vector2Int.One);
            }
            return new(min, max);
        }

        public static RectInt Intersection(RectInt left, RectInt right)
        {
            Vector2Int min = Vector2Int.Max(left.min, right.min);
            Vector2Int max = Vector2Int.Min(left.max, right.max);
            if (min.X > max.X || min.Y > max.Y) return default;
            return new(min, max);
        }

        public readonly bool Contains(Vector2Int point) =>
            point.X >= min.X && point.Y >= min.Y && point.X < max.X && point.Y < max.Y;

        public readonly bool Contains(RectInt rect) =>
            min.X <= rect.min.X && min.Y <= rect.min.Y && max.X > rect.max.X && max.Y > rect.max.Y;

        public readonly bool OverlapsWith(RectInt rect)
        {
            int minX = min.X >= rect.min.X ? min.X < rect.max.X ? 0 : 1 : -1;
            int minY = min.Y >= rect.min.Y ? min.Y < rect.max.Y ? 0 : 1 : -1;

            int maxX = max.X > rect.min.X ? max.X <= rect.max.X ? 0 : 1 : -1;
            int maxY = max.Y > rect.min.Y ? max.Y <= rect.max.Y ? 0 : 1 : -1;

            return (minX * maxX) < 1 && (minY * maxY) < 1;
        }

        public void UnionWith(RectInt rect)
        {
            min = Vector2Int.Min(min, rect.min);
            max = Vector2Int.Max(max, rect.max);
        }

        public void UnionWith(Vector2Int point)
        {
            min = Vector2Int.Min(min, point);
            max = Vector2Int.Max(max, point + Vector2Int.One);
        }

        public void UnionWithAll(params Vector2Int[] points) => UnionWithAll(collection: points);

        public void UnionWithAll(IEnumerable<Vector2Int> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            Vector2Int min = this.min, max = this.max;
            foreach (var point in collection)
            {
                min = Vector2Int.Min(min, point);
                max = Vector2Int.Max(max, point + Vector2Int.One);
            }
            this.min = min;
            this.max = max;
        }

        public void IntersectWith(RectInt rect)
        {
            Vector2Int min = Vector2Int.Max(this.min, rect.min);
            Vector2Int max = Vector2Int.Min(this.max, rect.max);
            if (min.X > max.X || min.Y > max.Y)
            {
                this.min = this.max = Vector2Int.Zero;
                return;
            }
            (this.min, this.max) = (min, max);
        }

        public readonly bool Equals(RectInt other) =>
            min == other.min && max == other.max;

        public readonly override bool Equals(object obj) =>
            obj is RectInt rect && Equals(rect);

        public readonly override int GetHashCode() => HashCode.Combine(typeof(RectInt), min, max);

        public readonly override string ToString() =>
            $"Rect({min.X}, {min.Y}; {max.X}, {max.Y})";

        public readonly IEnumerator<Vector2Int> GetEnumerator()
        {
            for (int y = min.Y; y < max.Y; ++y)
                for (int x = min.X; x < max.X; ++x)
                    yield return new(x, y);
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Width
        {
            readonly get => max.X - min.X;
            set
            {
                if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.X = checked(min.X + value);
            }
        }

        public int Height
        {
            readonly get => max.Y - min.Y;
            set
            {
                if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.Y = checked(min.Y + value);
            }
        }

        public Vector2Int Size
        {
            readonly get => new(Width, Height);
            set => (Width, Height) = value;
        }

        public int X
        {
            readonly get => min.X;
            set
            {
                int w = Width;
                max.X = checked(value + w);
                min.X = value;
            }
        }

        public int Y
        {
            readonly get => min.Y;
            set
            {
                int h = Height;
                max.Y = checked(value + h);
                min.Y = value;
            }
        }

        public Vector2Int Origin
        {
            readonly get => min;
            set
            {
                var s = Size;
                max = value + s;
                min = value;
            }
        }

        public int MinX
        {
            readonly get => min.X;
            set
            {
                if (value > max.X) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                min.X = value;
            }
        }

        public int MinY
        {
            readonly get => min.Y;
            set
            {
                if (value > max.Y) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                min.Y = value;
            }
        }

        public Vector2Int Min
        {
            readonly get => min;
            set
            {
                if (value.X > max.X) throw new ArgumentOutOfRangeException(nameof(value.X), $"Value out of range: {value.X}");
                if (value.Y > max.Y) throw new ArgumentOutOfRangeException(nameof(value.Y), $"Value out of range: {value.Y}");
                min = value;
            }
        }

        public int MaxX
        {
            readonly get => max.X;
            set
            {
                if (value < min.X) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.X = value;
            }
        }

        public int MaxY
        {
            readonly get => max.Y;
            set
            {
                if (value < min.Y) throw new ArgumentOutOfRangeException(nameof(value), $"Value out of range: {value}");
                max.Y = value;
            }
        }

        public Vector2Int Max
        {
            readonly get => max;
            set
            {
                if (value.X < min.X) throw new ArgumentOutOfRangeException(nameof(value.X), $"Value out of range: {value.X}");
                if (value.Y < min.Y) throw new ArgumentOutOfRangeException(nameof(value.Y), $"Value out of range: {value.Y}");
                max = value;
            }
        }

        public static RectInt operator |(RectInt left, RectInt right) => Union(left, right);
        public static RectInt operator |(RectInt left, Vector2Int right) => Union(left, right);
        public static RectInt operator |(Vector2Int left, RectInt right) => Union(right, left);
        public static RectInt operator &(RectInt left, RectInt right) => Intersection(left, right);

        public static bool operator ==(RectInt left, RectInt right) => left.Equals(right);
        public static bool operator !=(RectInt left, RectInt right) => !(left == right);

        public static implicit operator Rect(RectInt rect) => new(rect.min, rect.max);
        public static explicit operator RectInt(Rect rect) => new((Vector2Int)rect.Min, (Vector2Int)rect.Max);
    }
}
