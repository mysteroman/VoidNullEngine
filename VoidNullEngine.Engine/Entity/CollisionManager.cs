using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Entity
{
    public sealed class CollisionManager: IDisposable
    {
        private static readonly List<CollisionManager> managers;

        static CollisionManager() => managers = new List<CollisionManager>();

        public StaticEntity Owner { get; }
        public List<CollisionBox> CollisionBoxes { get; }

        public CollisionManager(StaticEntity owner)
        {
            Owner = owner;
            CollisionBoxes = new List<CollisionBox>();
            managers.Add(this);
        }

        public IEnumerable<(CollisionManager, CollisionBox)> IsColliding()
        {
            var collisions = GetCollisions();
            return from manager in managers
                   select new { Manager = manager, Boxes = manager.GetCollisions() } into manager
                   from box in manager.Boxes
                   where box.CollidesWithAny(collisions)
                   select (manager.Manager, box);
        }

        public Vector2 GetAllowedMotion()
        {
            if (Owner is DynamicEntity entity)
            {
                Vector2 motion = entity.MotionManager.Motion * GameTime.DeltaTime;
                IEnumerable<CollisionBox> self = from box in GetCollisions()
                           where box.CanCollideWith(entity.MotionManager.MotionBlock) && !box.IsBoundingBox
                           select box;

                if (motion.X < 0) motion.X = GetAllowedLeftMotion(self, -motion.X);
                else if (motion.X > 0) motion.X = GetAllowedRightMotion(self, motion.X);
                if (motion.Y < 0) motion.Y = GetAllowedUpMotion(self, -motion.Y);
                else if (motion.Y > 0) motion.Y = GetAllowedDownMotion(self, motion.Y);
                return motion;
            }
            return Vector2.Zero;
        }

        public void Dispose() => managers.Remove(this);

        private IEnumerable<CollisionBox> GetCollisions()
        {
            if (Owner is null) return CollisionBoxes;

            Matrix3x2 transforms;
            {
                Matrix3x2 rot = Matrix3x2.CreateRotation(Owner.Rotation);
                Matrix3x2 trans = Matrix3x2.CreateTranslation(Owner.Position);
                transforms = rot * trans;
            }
            return from box in CollisionBoxes select box.Transform(transforms);
        }

        private float GetAllowedLeftMotion(IEnumerable<CollisionBox> self, float motion) =>
            -GetAllowedDistance(self, GetLeftMotion, motion);
        private static float GetLeftMotion(CollisionBox s, CollisionBox o) =>
            o.IsBoundingBox ?
            s.TopLeft.X - o.TopLeft.X :
            s.TopLeft.X - o.BottomRight.X;

        private float GetAllowedRightMotion(IEnumerable<CollisionBox> self, float motion) =>
            GetAllowedDistance(self, GetRightMotion, motion);
        private static float GetRightMotion(CollisionBox s, CollisionBox o) =>
            o.IsBoundingBox ? 
            o.BottomRight.X - s.BottomRight.X :
            o.TopLeft.X - s.BottomRight.X;

        private float GetAllowedUpMotion(IEnumerable<CollisionBox> self, float motion) =>
            -GetAllowedDistance(self, GetUpMotion, motion);
        private static float GetUpMotion(CollisionBox s, CollisionBox o) =>
            o.IsBoundingBox ?
            s.TopLeft.Y - o.TopLeft.Y :
            s.TopLeft.Y - o.BottomRight.Y;

        private float GetAllowedDownMotion(IEnumerable<CollisionBox> self, float motion) =>
            GetAllowedDistance(self, GetDownMotion, motion);
        private static float GetDownMotion(CollisionBox s, CollisionBox o) =>
            o.IsBoundingBox ?
            o.BottomRight.Y - s.BottomRight.Y :
            o.TopLeft.Y - s.BottomRight.Y;

        private float GetAllowedDistance(IEnumerable<CollisionBox> self, Func<CollisionBox, CollisionBox, float> calculator, float motion)
        {
            float allowedMotion = motion;

            foreach (var s in self)
            {
                foreach (var manager in managers)
                {
                    if (manager == this) continue;
                    var other = from box in manager.GetCollisions()
                                where box.CanCollideWith(s.Flags)
                                select box;

                    foreach (var o in other)
                    {
                        allowedMotion = MathF.Min(calculator(s, o), allowedMotion);
                    }
                }
            }

            return allowedMotion;
        }
    }
}
