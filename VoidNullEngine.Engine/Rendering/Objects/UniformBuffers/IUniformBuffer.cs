using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Objects.UniformBuffers
{
    public interface IUniformBuffer
    {
        uint BufferID { get; }
        uint Binding { get; }
        bool IsStatic { get; }
    }
}
