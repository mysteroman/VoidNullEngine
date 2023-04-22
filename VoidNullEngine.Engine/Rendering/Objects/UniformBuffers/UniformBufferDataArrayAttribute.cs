using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Objects.UniformBuffers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class UniformBufferDataArrayAttribute : Attribute, IUniformBufferDataAttribute
    {
        public UniformBufferDataArrayAttribute(int offset, Type type, int maxItemCount)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must not be inferior to zero, is {offset}");
            if (offset % UniformBuffer.STRUCT_PADDING != 0) throw new ArgumentOutOfRangeException(nameof(offset), $"Offset must be a multiple of {UniformBuffer.STRUCT_PADDING}, is {offset}");
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (type.IsSZArray) ValueType = type.GetElementType();
            else if (type.IsConstructedGenericType)
            {
                var def = type.GetGenericTypeDefinition();
                if (def != typeof(Span<>) && def != typeof(ReadOnlySpan<>)) throw new ArgumentException($"{type} must be an Array or a Span type", nameof(type));
                ValueType = type.GenericTypeArguments[0];
            }
            else throw new ArgumentException($"{type} must be an Array or a Span type", nameof(type));
            if (maxItemCount < 1) throw new ArgumentOutOfRangeException(nameof(maxItemCount), $"Max item count must not be inferior to one, is {maxItemCount}");

            Offset = offset;
            ContainerType = type;

            ItemSize = UniformBuffer.SizeOf(ValueType);
            MaxItemCount = maxItemCount;

            Size = ItemSize * MaxItemCount;
        }

        public int Offset { get; }
        public Type ContainerType { get; }
        public Type ValueType { get; }
        public int Size { get; }

        public int ItemSize { get; }
        public int MaxItemCount { get; }
    }
}
