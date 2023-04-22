using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Entity
{
    public abstract class StaticEntity: IDisposable, IEquatable<StaticEntity>
    {
        public CollisionManager CollisionManager { get; protected init; }

        public readonly Guid ID = Guid.NewGuid();

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public abstract void Render();

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            CollisionManager?.Dispose();
        }

        public bool Equals(StaticEntity other) => ID.Equals(other.ID);

        public override bool Equals(object obj) =>
            obj is StaticEntity entity && Equals(entity);

        public override int GetHashCode() => ID.GetHashCode();
    }
}
