using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Math
{
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int X, Y;

        public static Vector2Int UnitX => new(1, 0);
        public static Vector2Int UnitY => new(0, 1);
        public static Vector2Int One => new(1, 1);
        public static Vector2Int Zero => new(0, 0);

        public Vector2Int(int value) => X = Y = value;
        public Vector2Int(int x, int y) => (X, Y) = (x, y);
        public Vector2Int(ReadOnlySpan<int> values)
        {
            if (values.IsEmpty) throw new ArgumentException("Memory is empty", nameof(values));
            if (values.Length == 1) X = Y = values[0];
            else (X, Y) = (values[0], values[1]);
        }

        public readonly void Deconstruct(out int x, out int y) => (x, y) = (X, Y);

        public readonly bool Equals(Vector2Int other) =>
            X == other.X && Y == other.Y;
        public readonly override bool Equals([NotNullWhen(true)] object obj) =>
            obj is Vector2Int v && Equals(v);
        public readonly override int GetHashCode() => HashCode.Combine(typeof(Vector2Int), X, Y);
        public readonly override string ToString() => $"({X}, {Y})";

        public static Vector2Int Abs(Vector2Int value) => new(System.Math.Abs(value.X), System.Math.Abs(value.Y));
        public static Vector2Int Add(Vector2Int left, Vector2Int right) => checked(new(left.X + right.X, left.Y + right.Y));
        public static Vector2Int Clamp(Vector2Int value, Vector2Int min, Vector2Int max) => new(System.Math.Clamp(value.X, min.X, max.X), System.Math.Clamp(value.Y, min.Y, max.Y));
        public static Vector2Int Divide(Vector2Int left, Vector2Int right) => new(left.X / right.X, left.Y / right.Y);
        public static Vector2Int Divide(Vector2Int left, int right) => new(left.X / right, left.Y / right);
        public static Vector2Int Max(Vector2Int left, Vector2Int right) => new(System.Math.Max(left.X, right.X), System.Math.Max(left.Y, right.Y));
        public static Vector2Int Min(Vector2Int left, Vector2Int right) => new(System.Math.Min(left.X, right.X), System.Math.Min(left.Y, right.Y));
        public static Vector2Int Multiply(Vector2Int left, Vector2Int right) => checked(new(left.X * right.X, left.Y * right.Y));
        public static Vector2Int Multiply(Vector2Int left, int right) => checked(new(left.X * right, left.Y * right));
        public static Vector2Int Multiply(int left, Vector2Int right) => checked(new(left * right.X, left * right.Y));
        public static Vector2Int Negate(Vector2Int value) => checked(new(-value.X, -value.Y));
        public static Vector2Int Subtract(Vector2Int left, Vector2Int right) => checked(new(left.X - right.X, left.Y - right.Y));

        public static Vector2Int operator +(Vector2Int left, Vector2Int right) => Add(left, right);
        public static Vector2Int operator -(Vector2Int value) => Negate(value);
        public static Vector2Int operator -(Vector2Int left, Vector2Int right) => Subtract(left, right);
        public static Vector2Int operator *(Vector2Int left, Vector2Int right) => Multiply(left, right);
        public static Vector2Int operator *(Vector2Int left, int right) => Multiply(left, right);
        public static Vector2 operator *(Vector2Int left, float right) => (Vector2)left * right;
        public static Vector2 operator *(float left, Vector2Int right) => left * (Vector2)right;
        public static Vector2Int operator *(int left, Vector2Int right) => Multiply(left, right);
        public static Vector2Int operator /(Vector2Int left, Vector2Int right) => Divide(left, right);
        public static Vector2Int operator /(Vector2Int left, int right) => Divide(left, right);
        public static Vector2 operator /(Vector2Int left, float right) => (Vector2)left / right;

        public static bool operator ==(Vector2Int left, Vector2Int right) => left.Equals(right);
        public static bool operator !=(Vector2Int left, Vector2Int right) => !(left == right);

        public static implicit operator Vector2(Vector2Int value) => new(value.X, value.Y);
        public static explicit operator Vector2Int(Vector2 value) => new((int)value.X, (int)value.Y);
    }
}
