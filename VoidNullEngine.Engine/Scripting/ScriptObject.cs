using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLua;
using VoidNullEngine.Engine.Utils;

namespace VoidNullEngine.Engine.Scripting
{
    public sealed class ScriptObject<T> : DynamicObject, IDisposable where T : class, IScriptObject, new()
    {
        private readonly Lua _lua;
        private bool _disposed = false;

        internal ScriptObject(ScriptClass<T> @class, Lua lua)
        {
            Class = @class;
            ObjectId = Guid.NewGuid();
            _lua = lua;
        }

        public ScriptClass<T> Class { get; }
        public Guid ObjectId { get; }

        public override bool Equals(object obj) =>
            obj is ScriptObject<T> o &&
            Class.Equals(o.Class) &&
            ObjectId.Equals(o.ObjectId);

        public override int GetHashCode() => HashCode.Combine(Class, ObjectId);

        public override string ToString() => $"ScriptObject<{typeof(T).Name}>:{ObjectId}";

        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            ScriptClass<T>.TryGetMember(_lua, binder.Name, out result);

        public override bool TrySetMember(SetMemberBinder binder, object value) =>
            ScriptClass<T>.TrySetMember(_lua, binder.Name, value);

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) =>
            ScriptClass<T>.TryInvokeMember(_lua, binder.Name, args, out result);

        public void Dispose()
        {
            if (!_disposed)
            {
                ScriptClass<T>.Dispose(_lua);
                _disposed = true;
            }
        }
    }
}
