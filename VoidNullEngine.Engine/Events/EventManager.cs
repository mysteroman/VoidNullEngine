using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Events
{
    public static class EventManager
    {
        private static readonly ConcurrentDictionary<Type, object> _managers;
        private static readonly ConcurrentQueue<Event> _events;

        static EventManager()
        {
            _managers = new ConcurrentDictionary<Type, object>();
            _events = new ConcurrentQueue<Event>();

            _managers.TryAdd(typeof(UpdateEvent), new EventManager<UpdateEvent>());
        }

        public static void RegisterEventType<E>() where E : Event
        {
            if (IsEventTypeRegistered<E>())
            {
                Debug.WriteLine($"Event {typeof(E)} is already registered");
                return;
            }
            if (EventManager<E>.ValidateDefinition())
            {
                _managers.TryAdd(typeof(E), new EventManager<E>());
                Debug.WriteLine($"Registered event {typeof(E)}");
            }
        }

        public static bool IsEventTypeRegistered<E>() where E : Event =>
            _managers.ContainsKey(typeof(E));

        public static void AddListener<E>(object listener, Action<E> method) where E : Event
        {
            if (_managers.TryGetValue(typeof(E), out var manager))
            {
                var handler = new Handler<E>(listener, method);
                (manager as EventManager<E>).AddListener(handler);
            }
        }

        public static void RemoveListener<E>(object listener, Action<E> method) where E : Event
        {
            if (_managers.TryGetValue(typeof(E), out var manager))
            {
                var handler = new Handler<E>(listener, method);
                (manager as EventManager<E>).RemoveListener(handler);
            }
        }

        public static void Send(Event @event) =>
            _events.Enqueue(@event);

        internal static void PollEvents()
        {
            while (_events.TryDequeue(out var e))
            {
                var eType = e.GetType();
                if (_managers.TryGetValue(eType, out var manager))
                {
                    
                }
            }
        }
    }

    internal class EventManager<E> where E: Event
    {
        private readonly List<Handler<E>> _handlers;

        public EventManager()
        {
            _handlers = new List<Handler<E>>();
        }

        public void AddListener(Handler<E> handler)
        {
            lock (_handlers)
            {
                if (!_handlers.Contains(handler)) _handlers.Add(handler);
            }
        }

        public void RemoveListener(Handler<E> handler)
        {
            lock (_handlers)
            {
                _handlers.Remove(handler);
            }
        }

        public void Send(E @event)
        {
            lock (_handlers)
            {
                foreach (var handler in _handlers)
                {
                    handler.Invoke(@event);
                }
            }
        }

        public void RemoveDeadHandlers()
        {
            lock (_handlers)
            {
                _handlers.RemoveAll(h => !h.IsAlive);
            }
        }

        public static bool ValidateDefinition()
        {
            Type type = typeof(E);

            bool ValidateFields()
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (!field.IsInitOnly)
                    {
                        Debug.WriteLine($"All fields in {type} must be readonly");
                        return false;
                    }
                }
                return true;
            }

            bool ValidateProperties()
            {
                Type initOnly = typeof(System.Runtime.CompilerServices.IsExternalInit);
                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var property in properties)
                {
                    if (!property.CanRead)
                    {
                        Debug.WriteLine($"All properties in {type} must be readable");
                        return false;
                    }

                    if (property.SetMethod is MethodInfo setter)
                    {
                        if (!setter.ReturnParameter.GetRequiredCustomModifiers().Contains(initOnly))
                        {
                            Debug.WriteLine($"All properties in {type} must be readable");
                            return false;
                        }
                    }
                }
                return true;
            }

            return ValidateFields() & ValidateProperties();
        }
    }
}
