using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Objects.UniformBuffers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class UniformBufferDataAttribute : Attribute, IUniformBufferDataAttribute
    {
        public UniformBufferDataAttribute(int offset, Type type, int size)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must not be inferior to zero, is {offset}");
            if (offset % UniformBuffer.STRUCT_PADDING != 0) throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must be a multiple of {UniformBuffer.STRUCT_PADDING}, is {offset}");
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size), $"Size must not be inferior to one, is {size}");

            Offset = offset;
            Size = size;
            size = UniformBuffer.SizeOf(ValueType = type);
            if (Size < size) throw new ArgumentOutOfRangeException(nameof(size), $"Size must not be inferior to the actual size of data, is {Size}, should be at least {size}");
        }

        public int Offset { get; }
        public Type ValueType { get; }
        public int Size { get; }
    }
}
