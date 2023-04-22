using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Core;

namespace VoidNullEngine.Engine.Assets.Animations
{
    public sealed class AnimationProperty : IEquatable<AnimationProperty>
    {
        private static readonly ConcurrentDictionary<KeyValuePair<Type, string>, PropertyInfo> properties = new();
        private static PropertyInfo GetProperty(KeyValuePair<Type, string> key) => key.Key.GetProperty(key.Value);
        private static readonly ConcurrentDictionary<KeyValuePair<Type, string>, FieldInfo> fields = new();
        private static FieldInfo GetField(KeyValuePair<Type, string> key) => key.Key.GetField(key.Value);

        private readonly string[] path;
        private readonly Func<object, object>[] resolveChain;

        public AnimationProperty(string[] path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Length == 0) throw new ArgumentException($"{nameof(path)} must not be empty", nameof(path));
            resolveChain = ConvertPath(this.path = path);
        }

        public Accessor Resolve(GameObject obj)
        {
            object o = obj;
            foreach (var link in resolveChain)
            {
                if (o is Accessor accessor)
                {
                    o = accessor.Value;
                    accessor.Dispose();
                }
                o = link(o);
                if (o is null) return null;
            }
            return o as Accessor;
        }

        public bool Equals(AnimationProperty other) =>
            other is not null && path.SequenceEqual(other.path);

        public override bool Equals(object obj) => Equals(obj as AnimationProperty);

        public override int GetHashCode() => path.GetHashCode();

        public override string ToString() => string.Join("->", path);

        public static bool operator ==(AnimationProperty left, AnimationProperty right) =>
            left is null ? right is null : left.Equals(right);
        public static bool operator !=(AnimationProperty left, AnimationProperty right) =>
            !(left == right);

        private static Func<object, object>[] ConvertPath(string[] path)
        {
            var result = new Func<object, object>[path.Length];
            for (int i = 0; i < path.Length; ++i)
            {
                bool writable = i == path.Length - 1;
                var p = path[i];

                if (!writable)
                {
                    if (p.StartsWith('#'))
                    {
                        var id = Guid.Parse(p.AsSpan(1));
                        result[i] = ResolveChild(id);
                        continue;
                    }
                }

                result[i] = ResolveProperty(p, writable);
            }
            return result;
        }

        private static Func<object, object> ResolveChild(Guid id) =>
            obj => obj is GameObject go ? go.FindChild(id) : null;

        private static Func<object, object> ResolveProperty(string name, bool writable)
        {
            object Get(object obj)
            {
                var type = obj.GetType();
                var key = new KeyValuePair<Type, string>(type, name);

                PropertyInfo prop = properties.GetOrAdd(key, GetProperty);
                if (prop is not null)
                {
                    var getMethod = prop.GetGetMethod();
                    if (getMethod is null) return null;
                    var getter = getMethod.CreateDelegate<Func<object>>(obj);

                    var setMethod = prop.GetSetMethod();
                    if (setMethod is null && writable) return null;
                    var setter = setMethod?.CreateDelegate<Action<object>>(obj);

                    return new Accessor(prop.PropertyType, setter, getter);
                }

                FieldInfo field = fields.GetOrAdd(key, GetField);
                if (field is not null)
                {
                    object getter() => field.GetValue(obj);
                    void setter(object value) => field.SetValue(obj, value);

                    return new Accessor(field.FieldType, setter, getter);
                }

                return null;
            }
            return Get;
        }

        public sealed class Accessor : IDisposable
        {
            private Action<object> setter;
            private Func<object> getter;

            internal Accessor(Type type, Action<object> setter, Func<object> getter)
            {
                ValueType = type;
                this.setter = setter;
                this.getter = getter;
            }

            ~Accessor() => Dispose();

            public Type ValueType { get; }

            public object Value
            {
                get => getter();
                set
                {
                    if (setter == null) return;
                    if (!IsValueAssignable(value)) return;
                    setter(value);
                }
            }

            public bool IsValueAssignable(object value) =>
                value is null ? !ValueType.IsValueType : ValueType.IsAssignableFrom(value.GetType());

            public void Dispose()
            {
                setter = null;
                getter = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
