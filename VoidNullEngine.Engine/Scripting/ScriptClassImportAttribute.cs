using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Scripting
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ScriptClassImportAttribute : Attribute
    {
        public readonly string Namespace;
        public readonly string Assembly;

        public ScriptClassImportAttribute(string @namespace, string assembly = null) => 
            (Namespace, Assembly) = (@namespace, assembly);
    }
}
