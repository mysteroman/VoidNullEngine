using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering.Models;

namespace VoidNullEngine.Engine.Utils
{
    public static class Extensions
    {
        public static float[] GetArray(this Matrix4x4 m)
        {
            return new float[]
            {
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44
            };
        }

        public static float[] GetArray(this Matrix3x2 m)
        {
            return new float[]
            {
                m.M11, m.M12, 0,
                m.M21, m.M22, 0,
                m.M31, m.M32, 1
            };
        }

        public static Type[] GetParameterTypes(this MethodBase method) =>
            (from param in method.GetParameters()
             select param.ParameterType).ToArray();

        public static Type[] GetTypes(this object[] objects) =>
            (from obj in objects select obj.GetType()).ToArray();

        public static ISet<Vector2> Transform(this ISet<Vector2> points, Matrix3x2 matrix) =>
            new HashSet<Vector2>(from point in points select Vector2.Transform(point, matrix));

        public static double NextGaussian(this Random rand, double std = 1)
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double deviation = System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Sin(2 * System.Math.PI * u2);
            return std * deviation;
        }
    }
}
