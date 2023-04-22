using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public abstract class AbstractNoiseGenerator : INoiseGenerator
    {
        public readonly int Seed;

        public abstract float MinValue { get; }
        public abstract float MaxValue { get; }

        public AbstractNoiseGenerator(int seed) => Seed = seed;

        public abstract float Generate(float x, float y = 0, float z = 0);

        public virtual float GenerateNormal(float x, float y = 0, float z = 0)
        {
            float noise = Generate(x, y, z);
            return (noise - MinValue) / (MaxValue - MinValue);
        }
        public abstract (float, float, float) GetCoordsInLocalSpace(float x, float y, float z);
        public abstract (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z);
    }
}
