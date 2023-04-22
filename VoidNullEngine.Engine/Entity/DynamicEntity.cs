using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Events;

namespace VoidNullEngine.Engine.Entity
{
    public abstract class DynamicEntity : StaticEntity
    {
        public MotionManager MotionManager { get; protected init; }
        
        public DynamicEntity()
        {
            EventManager.AddListener<UpdateEvent>(this, Update);
        }

        public abstract void Update(UpdateEvent @event);
    }
}
