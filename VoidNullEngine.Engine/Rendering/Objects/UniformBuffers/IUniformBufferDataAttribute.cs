using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Objects.UniformBuffers
{
    internal interface IUniformBufferDataAttribute
    {
        int Offset { get; }
        Type ValueType { get; }
        int Size { get; }
    }
}
