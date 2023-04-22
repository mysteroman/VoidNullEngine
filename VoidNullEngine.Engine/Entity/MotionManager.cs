using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Entity
{
    public sealed class MotionManager
    {
        public const float DEFAULT_TOP_SPEED = 320f;
        public const float DEFAULT_MAX_ACCELERATION = float.MaxValue;
        public const float DEFAULT_BASE_DECELERATION = float.MaxValue;
        public const float DEFAULT_TRACTION = 1f;
        public const float DEFAULT_DRIFT = 0f;
        public const float DEFAULT_MASS = 1f;
        public const float DEFAULT_PRECISION = 1e-2f;
        public const float DEFAULT_MIN_SPEED = 1e-3f;

        private const float DOWN_FORCE = 10;

        private Vector2 Movement;
        private Vector2 Knockback;

        public float Precision { get; set; }
        public float MinSpeed { get; set; }
        public float MaxAcceleration { get; set; }
        public float BaseDeceleration { get; set; }
        public float TopSpeed { get; set; }
        public float Mass { get; set; }
        public float Traction { get; set; }
        public float Drift { get; set; }
        public CollisionFlag MotionBlock { get; set; }

        public Vector2 Motion => HasKnockback ? Knockback : Movement;

        public bool IsMoving => float.IsNormal(Motion.Length());
        public bool HasKnockback => float.IsNormal(Knockback.Length());

        public MotionManager()
        {
            Movement = Vector2.Zero;
            Knockback = Vector2.Zero;

            Precision = DEFAULT_PRECISION;
            MinSpeed = DEFAULT_MIN_SPEED;
            MaxAcceleration = DEFAULT_MAX_ACCELERATION;
            BaseDeceleration = DEFAULT_BASE_DECELERATION;
            TopSpeed = DEFAULT_TOP_SPEED;
            Mass = DEFAULT_MASS;
            Traction = DEFAULT_TRACTION;
            Drift = DEFAULT_DRIFT;
        }

        #region Movement
        public void ComputeMovement(float force, Vector2 direction)
        {
            if (Traction <= 0) return;
            if (HasKnockback || !float.IsNormal(force) || !float.IsNormal(direction.LengthSquared())) return;

            float traction = Traction > 1 ? 1 : Traction;
            force = MathF.Min(force, MaxAcceleration) * traction;

            direction = Vector2.Normalize(direction);
            float acceleration = force * GameTime.DeltaTime;
            float lastSpeed = Movement.Length();

            if (!float.IsNormal(lastSpeed))
            {
                Movement = direction * MathF.Min(acceleration, TopSpeed);
                Sanitize(ref Movement);
                return;
            }

            var resistance = new Vector2(direction.Y, -direction.X);
            float friction = Vector2.Dot(Movement, resistance);
            friction = MathF.CopySign(MathF.Min(MathF.Abs(friction), Traction * GameTime.DeltaTime * BaseDeceleration), friction);
            Movement -= resistance * friction;

            Movement += direction * acceleration;
            Movement = Vector2.Normalize(Movement) * MathF.Min(Movement.Length(), TopSpeed);
            Sanitize(ref Movement);
        }

        public void ComputeMovement(float force, float direction)
        {
            if (Traction <= 0) return;
            if (HasKnockback || !float.IsNormal(force) || !float.IsFinite(direction)) return;

            float traction = Traction > 1 ? 1 : Traction;
            force = MathF.Min(force, MaxAcceleration) * traction;

            var aDir = new Vector2(MathF.Cos(direction), MathF.Sin(direction));
            float acceleration = force * GameTime.DeltaTime;
            float lastSpeed = Movement.Length();

            if (!float.IsNormal(lastSpeed))
            {
                Movement = aDir * MathF.Min(acceleration, TopSpeed);
                Sanitize(ref Movement);
                return;
            }

            var resistance = new Vector2(aDir.Y, -aDir.X);
            float friction = Vector2.Dot(Movement, resistance);
            friction = MathF.CopySign(MathF.Min(MathF.Abs(friction), Traction * GameTime.DeltaTime * BaseDeceleration), friction);
            Movement -= resistance * friction;

            Movement += aDir * acceleration;
            Movement = Vector2.Normalize(Movement) * MathF.Min(Movement.Length(), TopSpeed);
            Sanitize(ref Movement);
        }

        public void DecelerateMovement()
        {
            if (Traction <= 0) return;
            float motion = Movement.Length();
            if (!float.IsNormal(motion)) return;
            float deceleration = BaseDeceleration * GameTime.DeltaTime * Traction;
            if (deceleration >= motion)
            {
                Movement = Vector2.Zero;
                return;
            }
            Movement -= Vector2.Normalize(Movement) * deceleration;
            Sanitize(ref Movement);
        }

        #endregion
        #region Knockback

        public void ComputeKnockback(Vector2 impulse)
        {
            if (!float.IsNormal(impulse.LengthSquared())) return;
            if (Traction > 0)
            {
                float friction = Traction * Mass * DOWN_FORCE;
                if (impulse.Length() <= friction) return;
            }

            impulse /= Mass;
            if (float.IsNormal(Movement.LengthSquared()))
            {
                impulse += Movement;
                Movement = Vector2.Zero;
            }
            Knockback += impulse;
            Sanitize(ref Knockback);
        }

        public void ComputeKnockback(float impulse, float direction)
        {
            if (!float.IsNormal(impulse) || !float.IsFinite(direction)) return;
            if (Traction > 0)
            {
                float friction = Traction * Mass * DOWN_FORCE;
                if (impulse <= friction) return;
                impulse -= friction;
            }

            impulse /= Mass;
            Vector2 knockback = new Vector2(MathF.Cos(direction), MathF.Sin(direction)) * impulse;
            if (float.IsNormal(Movement.LengthSquared()))
            {
                knockback += Movement;
                Movement = Vector2.Zero;
            }
            Knockback += knockback;
            Sanitize(ref Knockback);
        }

        public void DriftKnockback(Vector2 movementDirection)
        {
            if (Drift <= 0) return;
            if (!HasKnockback) return;
            if (!float.IsNormal(movementDirection.LengthSquared())) return;

            var mDir = Vector2.Normalize(movementDirection);
            var kDir = Vector2.Normalize(Knockback);
            float drift = MathF.Sqrt(1 - MathF.Pow(Vector2.Dot(mDir, kDir), 2)) * Drift * GameTime.DeltaTime;
            drift = MathF.Min(drift, 1);

            Knockback = Vector2.Lerp(kDir, mDir, drift) * Knockback.Length();
            Sanitize(ref Knockback);
        }

        public void DriftKnockback(float movementDirection)
        {
            if (Drift <= 0) return;
            if (!HasKnockback) return;
            if (!float.IsFinite(movementDirection)) return;

            var mDir = new Vector2(MathF.Cos(movementDirection), MathF.Sin(movementDirection));
            var kDir = Vector2.Normalize(Knockback);
            float drift = MathF.Sqrt(1 - MathF.Pow(Vector2.Dot(mDir, kDir), 2)) * Drift * GameTime.DeltaTime;
            drift = MathF.Min(drift, 1);

            Knockback = Vector2.Lerp(kDir, mDir, drift) * Knockback.Length();
            Sanitize(ref Knockback);
        }

        public void ComputeKnockback()
        {
            if (Traction <= 0) return;
            if (!HasKnockback) return;

            float friction = Traction * DOWN_FORCE * GameTime.DeltaTime;
            if (Knockback.Length() <= friction)
            {
                Knockback = Vector2.Zero;
                return;
            }
            Knockback -= Vector2.Normalize(Knockback) * friction;
            Sanitize(ref Knockback);
        }

        #endregion

        private void Sanitize(ref Vector2 motion)
        {
            float length = motion.Length();
            if (length < MinSpeed)
            {
                motion = Vector2.Zero;
                return;
            }

            if (MathF.Abs(motion.X / length) < Precision)
            {
                motion.X = 0;
                motion.Y = MathF.CopySign(length, motion.Y);
                return;
            }

            if (MathF.Abs(motion.Y / length) < Precision)
            {
                motion.Y = 0;
                motion.X = MathF.CopySign(length, motion.X);
                return;
            }
        }
    }
}
