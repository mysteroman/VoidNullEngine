using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Input
{
    public abstract class Controller
    {
        internal abstract ControllerInput Query();
    }
}
