using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Models
{
    public ref struct Vertex
    {
        public const int Size = 4;

        private readonly Span<float> data;

        internal Vertex(Span<float> triangle, int vertexNum)
        {
            int index = vertexNum * Size;
            data = triangle.Slice(index, Size);
        }

        public float X
        {
            readonly get => data[0];
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(X)} coordinate must be finite, is {value}");
                data[0] = value;
            }
        }

        public float Y
        {
            readonly get => data[1];
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(Y)} coordinate must be finite, is {value}");
                data[1] = value;
            }
        }

        public float TextureX
        {
            readonly get => data[2];
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(TextureX)} coordinate must be finite, is {value}");
                data[2] = value;
            }
        }

        public float TextureY
        {
            readonly get => data[3];
            set
            {
                if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(TextureY)} coordinate must be finite, is {value}");
                data[3] = value;
            }
        }

        public ReadOnlySpan<float> AsSpan() => data;
    }
}
