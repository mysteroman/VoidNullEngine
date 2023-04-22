using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Events
{
    public abstract class Event
    {
        public object Source { get; }
        public Selector Selector { get; }

        public Event(object source = null, Selector selector = null)
        {
            Source = source;
            Selector = selector ?? SelectAll;
        }

        private static bool SelectAll(object target) => true;
    }
}
