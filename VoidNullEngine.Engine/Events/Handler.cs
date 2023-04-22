using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Events
{
    public sealed class Handler<E> : IEquatable<Handler<E>> where E : Event
    {
        private readonly WeakReference _listener;
        private readonly MethodInfo _method;

        public Handler(object listener, Action<E> method)
        {
            if (listener is null) throw new ArgumentNullException(nameof(listener));
            if (method is null) throw new ArgumentNullException(nameof(method));
            if (method.Target != listener) throw new ArgumentException("The given method's target and the given listener are not the same");

            _listener = new WeakReference(listener);
            _method = method.Method;
        }

        public Handler(Action<E> method) : this(method.Target, method)
        {

        }

        public bool IsAlive => _listener.IsAlive;

        public void Invoke(E @event)
        {
            if (@event is null) return;
            if (_listener.Target is object target && @event.Selector(target))
            {
                _method.Invoke(target, new object[] { @event });
            }
        }

        public bool Equals(Handler<E> other)
        {
            if (other is null) return false;
            if (_listener.Target is object target && ReferenceEquals(target, other._listener.Target))
            {
                return EqualityComparer<MethodInfo>.Default.Equals(_method, other._method);
            }
            return false;
        }

        public override bool Equals(object obj) => Equals(obj as Handler<E>);

        public override int GetHashCode() => HashCode.Combine(_listener.Target, _method);
    }
}
