using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Models
{
    public sealed class ModelData
    {
        private float[] data;

        public ModelData(int triangles)
        {
            if (triangles < 1) throw new ArgumentOutOfRangeException(nameof(triangles), $"Model must minimally be a triangle");
            data = new float[checked(triangles * Triangle.Size)];
        }

        public ModelData() : this(1) { }

        public int TriangleCount => data.Length / Triangle.Size;
        public int VertexCount => data.Length / Vertex.Size;
        public int Size => data.Length;

        public Triangle this[Index triangle]
        {
            get
            {
                int i = triangle.GetOffset(TriangleCount);
                if (i < 0 || i >= TriangleCount) throw new ArgumentOutOfRangeException(nameof(triangle), $"Index must be between 0 and {TriangleCount}, is {i}");
                return new(data, i);
            }
        }

        public void AddTriangles(int count)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), $"Number of added triangles must be at least 1");
            int newSize = checked(count * Triangle.Size + data.Length);
            Array.Resize(ref data, newSize);
        }

        public Triangle AddTriangle()
        {
            AddTriangles(1);
            return this[^1];
        }

        public void CopyTo(float[] array, int startIndex) => data.CopyTo(array, startIndex);

        public float[] ToArray()
        {
            var copy = new float[data.Length];
            CopyTo(copy, 0);
            return copy;
        }

        public override string ToString()
        {
            StringBuilder sb = new($"{nameof(ModelData)}[{data[0]}");
            for (int i = 1; i < data.Length; ++i)
                sb.Append(", ").Append(data[i]);
            sb.Append(']');
            return sb.ToString();
        }

        public ReadOnlySpan<float> AsSpan() => data.AsSpan();
    }
}
