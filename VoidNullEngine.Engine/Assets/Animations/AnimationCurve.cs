using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Core;

namespace VoidNullEngine.Engine.Assets.Animations
{
    public struct AnimationCurve
    {
        private static readonly CurveFunction DEFAULT_CURVE = LinearCurve;
        private static readonly InterpolationFunction DEFAULT_INTERP = Interpolate;

        private CurveFunction curve;
        private InterpolationFunction interp;
        
        public CurveFunction Curve
        {
            readonly get => curve ?? DEFAULT_CURVE;
            set => curve = value ?? DEFAULT_CURVE;
        }

        public InterpolationFunction InterpFunction
        {
            readonly get => interp ?? DEFAULT_INTERP;
            set => interp = value ?? DEFAULT_INTERP;
        }
        
        public readonly void Animate(float normalizedTime, object startValue, object endValue) =>
            InterpFunction(Curve(normalizedTime), startValue, endValue);

        public delegate object InterpolationFunction(float normalizedTime, object startValue, object endValue);
        public delegate float CurveFunction(float normalizedTime);

        #region Curves
        public static float FlatCurve(float x) => x < 1f ? 0 : 1;
        public static float LinearCurve(float x) => x;
        #endregion
        #region Interpolations
        public static object Interpolate(float x, object start, object end)
        {
            if (start is float f1 && end is float f2) return FloatInterp(x, f1, f2);
            if (start is double d1 && end is double d2) return DoubleInterp(x, d1, d2);

            return NearestInterp(x, start, end);
        }

        public static object NoInterp(float x, object start, object end) => x < 1f ? start : end;
        public static object NearestInterp(float x, object start, object end) => x < 0.5f ? start : end;

        public static float FloatInterp(float x, float start, float end) => start + x * (end - start);
        public static double DoubleInterp(float x, double start, double end) => start + x * (end - start);
        public static int IntInterp(float x, int start, int end) => (int)(start + x * (end - start));
        public static long LongInterp(float x, long start, long end) => (long)(start + x * (end - start));
        #endregion
    }
}
