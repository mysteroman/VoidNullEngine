using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Models
{
    public ref struct Triangle
    {
        public const int VertexCount = 3;
        public const int Size = Vertex.Size * VertexCount;

        private readonly Span<float> data;

        internal Triangle(Span<float> model, int triangleNum)
        {
            int index = triangleNum * Size;
            data = model.Slice(index, Size);
        }

        public readonly Vertex V0 => new(data, 0);
        public readonly Vertex V1 => new(data, 1);
        public readonly Vertex V2 => new(data, 2);

        public readonly Vertex this[Index vertex]
        {
            get
            {
                int i = vertex.GetOffset(VertexCount);
                return i switch
                {
                    0 => V0,
                    1 => V1,
                    2 => V2,
                    _ => throw new ArgumentOutOfRangeException(nameof(vertex), $"Index must be between 0 and {VertexCount}, is {i}")
                };
            }
        }

        public ReadOnlySpan<float> AsSpan() => data;
    }
}
