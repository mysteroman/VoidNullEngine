using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    internal abstract class NetworkEntity
    {
        public int ClientId { get; protected set; }

        protected NetworkEntity() {}

        protected internal abstract void Close();
    }
}
