using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering.Objects.UniformBuffers
{
    public static class UniformBuffer
    {
        internal const int STRUCT_PADDING = 8;
        internal const int LAYOUT_PADDING = 16;

        public static int SizeOf(Type type)
        {
            var buffer = Find(type);
            return buffer?.Size ?? throw new ArgumentException($"{type} is not valid inside a UniformBuffer", nameof(type));
        }

        #region Utility
        private static readonly ConcurrentDictionary<Type, BufferPrimitive> primitives = new();
        private static readonly ConcurrentDictionary<Type, BufferType> types = new();

        private static IBufferType Find(Type type)
        {
            if (primitives.TryGetValue(type, out var primitive)) return primitive;
            return types.GetOrAdd(type, RegisterBufferTypeNoThrow);
        }

        private static BufferType RegisterBufferType(Type type)
        {
            if (!type.IsClass && !type.IsValueType) throw new ArgumentException($"Buffer type {type} must be a class or a struct", nameof(type));
            if (type.IsAbstract) throw new ArgumentException($"Buffer type {type} must not be abstract", nameof(type));
            if (type.IsGenericType) throw new ArgumentException($"Buffer type {type} must not be generic", nameof(type));

            var properties = ValidateBufferSubData(type);

            return null;
        }

        private static BufferType RegisterBufferTypeNoThrow(Type type)
        {
            try
            {
                return RegisterBufferType(type);
            }
            catch
            {
                return null;
            }
        }

        private static List<BufferSubData> ValidateBufferSubData(Type type)
        {
            List<BufferSubData> data = new();

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties)
            {
                var info = property.GetCustomAttribute<UniformBufferDataAttribute>();
                if (info is null) continue;
                data.Add(ValidateBufferSubData(property, info));
            }

            data.Sort();
            return data;
        }

        private static BufferSubData ValidateBufferSubData(PropertyInfo property, UniformBufferDataAttribute info)
        {
            
        }

        private static uint nextBindingId = 0;

        private static uint NextBinding() => Interlocked.Increment(ref nextBindingId);

        private static void CreateBuffer(BufferType info, out uint buffer, out uint binding)
        {
            buffer = glGenBuffer();
            glBindBuffer(GL_UNIFORM_BUFFER, buffer);
            glBufferData(GL_UNIFORM_BUFFER, info.size, IntPtr.Zero, info.isStatic ? GL_STATIC_DRAW : GL_DYNAMIC_DRAW);
            glBindBuffer(GL_UNIFORM_BUFFER, 0);

            glBindBufferBase(GL_UNIFORM_BUFFER, binding = NextBinding(), buffer);
        }

        private interface IBufferType
        {
            int Size { get; }
        }

        private abstract class BufferPrimitive : IBufferType
        {
            public BufferPrimitive(int size) => Size = size;

            public int Size { get; }

            public abstract void Push<T>(int offset, T value) where T : unmanaged;
        }

        private class BufferType : IBufferType
        {
            public int Size { get; }
            public readonly List<BufferSubData> properties;

            public BufferType(int size, List<BufferSubData> properties)
            {
                Size = size;
                this.properties = properties;
            }
        }

        private abstract class BufferSubData : IComparable<BufferSubData>
        {
            public readonly IUniformBufferDataAttribute info;
            protected BufferSubData(IUniformBufferDataAttribute info)
            {
                this.info = info;
            }

            public abstract void Push(object instance);

            public int CompareTo(BufferSubData other) => info.Offset - other.info.Offset;
        }

        private class BufferSubData<T> : BufferSubData
        {
            public readonly Func<object, T> getter;
            public readonly Action<T> pusher;

            public BufferSubData(IUniformBufferDataAttribute info, Func<object, T> getter, Action<T> pusher) : base(info)
            {
                this.getter = getter;
                this.pusher = pusher;
            }

            public override void Push(object instance)
            {
                pusher(getter(instance));
            }
        }
        #endregion
        #region Primitives
        private class SinglePrimitive<T> : BufferPrimitive where T : unmanaged
        {
            public SinglePrimitive() : base(Marshal.SizeOf<T>())
            {
            }

            public override unsafe void Push<T1>(int offset, T1 value)
            {
                if (value is not T v) throw new InvalidCastException($"Trying to cast {typeof(T1)} to {typeof(T)}");
                glBufferSubData(GL_UNIFORM_BUFFER, offset, Size, &v);
            }
        }

        private class SpanPrimitive<T, R> : BufferPrimitive where T : unmanaged where R : unmanaged
        {
            public delegate ReadOnlySpan<R> SpanConverter(T value);
            private readonly SpanConverter converter;

            public SpanPrimitive(SpanConverter converter, int size) : base(Marshal.SizeOf<T>() * size)
            {
                this.converter = converter;
            }

            public override unsafe void Push<T1>(int offset, T1 value)
            {
                if (value is not T v) throw new InvalidCastException($"Trying to cast {typeof(T1)} to {typeof(T)}");
                fixed (R* ptr = converter(v)) glBufferSubData(GL_UNIFORM_BUFFER, offset, Size, ptr);
            }
        }

        



        static UniformBuffer()
        {
            primitives.TryAdd(typeof(float), new SinglePrimitive<float>());
            primitives.TryAdd(typeof(int), new SinglePrimitive<int>());
            primitives.TryAdd(typeof(Vector2), new SinglePrimitive<Vector2>());
            primitives.TryAdd(typeof(Vector3), new SinglePrimitive<Vector3>());
            primitives.TryAdd(typeof(Vector4), new SinglePrimitive<Vector4>());
            primitives.TryAdd(typeof(Matrix4x4), new SinglePrimitive<Matrix4x4>());
        }
        #endregion
    }
}
