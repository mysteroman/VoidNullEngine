using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public sealed class OctaveNoiseGenerator : INoiseGenerator
    {
        public readonly INoiseGenerator Source;
        public readonly int Octaves;
        public readonly float Persistence;

        public OctaveNoiseGenerator(INoiseGenerator source, int octaves, float persistence)
        {
            Source = source;
            if (octaves < 1) throw new ArgumentOutOfRangeException(nameof(octaves), "The number of octaves must be above zero");
            Octaves = octaves;
            if (!float.IsNormal(persistence)) throw new ArgumentException("Persistence must be a normal value");
            Persistence = persistence;
        }

        public float MinValue => Source.MinValue;
        public float MaxValue => Source.MaxValue;

        public float Generate(float x, float y = 0, float z = 0)
        {
            var (xl, yl, zl) = GetCoordsInLocalSpace(x, y, z);
            (x, y, z) = GetCoordsInGlobalSpace(x, y, z);

            float total = 0;
            float frequency = 1;
            float amplitude = 1;

            for (int o = 0; o < Octaves; ++o)
            {
                total += Source.Generate(x + xl / frequency, y + yl / frequency, z + zl / frequency) * amplitude;

                amplitude *= Persistence;
                frequency *= 2;
            }

            return total;
        }

        public float GenerateNormal(float x, float y = 0, float z = 0)
        {
            var (xl, yl, zl) = GetCoordsInLocalSpace(x, y, z);
            (x, y, z) = GetCoordsInGlobalSpace(x, y, z);

            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;

            for (int o = 0; o < Octaves; ++o)
            {
                total += Source.GenerateNormal(x + xl / frequency, y + yl / frequency, z + zl / frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= Persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        public override bool Equals(object obj) =>
            obj is OctaveNoiseGenerator noise &&
            Source.Equals(noise.Source) &&
            Octaves == noise.Octaves &&
            Persistence == noise.Persistence;

        public override int GetHashCode() =>
            HashCode.Combine(typeof(OctaveNoiseGenerator), Source, Octaves, Persistence);

        public (float, float, float) GetCoordsInLocalSpace(float x, float y, float z) =>
            Source.GetCoordsInLocalSpace(x, y, z);

        public (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z) =>
            Source.GetCoordsInGlobalSpace(x, y, z);
    }
}
