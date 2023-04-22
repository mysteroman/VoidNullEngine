using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public sealed class ValueNoiseGenerator : AbstractNoiseGenerator
    {
        private const float MAX_VALUE = (int.MaxValue - 1f) / int.MaxValue;
        
        public ValueNoiseGenerator() : this((int)DateTime.Now.Ticks)
        {
        }

        public ValueNoiseGenerator(int seed) : base(seed)
        {
        }

        public override float MinValue => 0f;
        public override float MaxValue => MAX_VALUE;

        public override float Generate(float x, float y = 0, float z = 0)
        {
            var r = new Random(HashCode.Combine(Seed, x, y, z));
            return (float)r.NextDouble();
        }

        public override (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z) => ((int)x, (int)y, (int)z);
        public override (float, float, float) GetCoordsInLocalSpace(float x, float y, float z) => (x % 1, y % 1, z % 1);
    }
}
