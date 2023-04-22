using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    /**
     * <summary>
     * Represents the combination of two <see cref="INoiseGenerator"/>s
     * </summary>
     */
    public sealed class CompoundNoiseGenerator : INoiseGenerator
    {
        private readonly INoiseGenerator A, B;
        private readonly Func<float, float, float> MergeFunction;

        internal CompoundNoiseGenerator(INoiseGenerator a, INoiseGenerator b, Func<float, float, float> merge) =>
            (A, B, MergeFunction) = (a, b, merge);

        public float MinValue => MergeFunction(A.MinValue, B.MinValue);
        public float MaxValue => MergeFunction(A.MaxValue, B.MaxValue);

        public float Generate(float x, float y = 0, float z = 0) => MergeFunction(A.Generate(x, y, z), B.Generate(x, y, z));
        public float GenerateNormal(float x, float y = 0, float z = 0) => 
            (Generate(x, y, z) - MinValue) / (MaxValue - MinValue);

        public (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z) => ((int)x, (int)y, (int)z);
        public (float, float, float) GetCoordsInLocalSpace(float x, float y, float z) => (x % 1, y % 1, z % 1);
    }
}
