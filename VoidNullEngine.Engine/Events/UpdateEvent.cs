using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Events
{
    public sealed class UpdateEvent : Event
    {
        internal UpdateEvent() : base(null, null)
        {
        }
    }
}
