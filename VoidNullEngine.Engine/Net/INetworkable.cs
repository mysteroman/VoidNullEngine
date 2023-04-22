using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    public interface INetworkable
    {
        Guid ClientId { get; }
        Guid NetworkId { get; }
    }
}
