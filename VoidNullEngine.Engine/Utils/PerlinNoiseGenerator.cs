using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public sealed class PerlinNoiseGenerator : AbstractNoiseGenerator
    {
        private readonly int[] p;

        private static int[] GeneratePermutations(int seed)
        {
            List<int> list = new List<int>();
            Random r = new Random(seed);
            for (int i = 0; i < 256; ++i) list.Insert(r.Next(list.Count), i);
            return list.ToArray();
        }

        public PerlinNoiseGenerator(int seed) : base(seed)
        {
            p = GeneratePermutations(Seed);
            Repeat = 0;
        }

        public PerlinNoiseGenerator() : this((int) DateTime.Now.Ticks) { }

        public override float MinValue => -1f;
        public override float MaxValue => 1f;

        private int _repeat;
        public int Repeat { get => _repeat; set => _repeat = System.Math.Clamp(value, 0, 255); }

        public override (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z)
        {
            float xi = (int)ToGlobalSpace(x);
            float yi = (int)ToGlobalSpace(y);
            float zi = (int)ToGlobalSpace(z);

            return (xi, yi, zi);
        }

        public override (float, float, float) GetCoordsInLocalSpace(float x, float y, float z)
        {
            x = ToGlobalSpace(x);
            y = ToGlobalSpace(y);
            z = ToGlobalSpace(z);

            x -= (int)x;
            y -= (int)y;
            z -= (int)z;

            return (x, y, z);
        }
        private float ToGlobalSpace(float v)
        {
            v %= p.Length;
            if (v < 0) v += p.Length;
            if (Repeat > 0) v %= Repeat;
            return v;
        }

        public override float Generate(float x, float y = 0, float z = 0)
        {
            var (xf, yf, zf) = GetCoordsInLocalSpace(x, y, z);
            (x, y, z) = GetCoordsInGlobalSpace(x, y, z);

            int xi = (int)x;
            int yi = (int)y;
            int zi = (int)z;

            float u = Fade(xf);
            float v = Fade(yf);
            float w = Fade(zf);

            int aaa = Hash(xi, yi, zi);
            int aba = Hash(xi, Inc(yi), zi);
            int aab = Hash(xi, yi, Inc(zi));
            int abb = Hash(xi, Inc(yi), Inc(zi));
            int baa = Hash(Inc(xi), yi, zi);
            int bba = Hash(Inc(xi), Inc(yi), zi);
            int bab = Hash(Inc(xi), yi, Inc(zi));
            int bbb = Hash(Inc(xi), Inc(yi), Inc(zi));

            float x1, x2, y1, y2;

            x1 = Lerp(Gradient(aaa, xf, yf, zi), Gradient(baa, xf - 1, yf, zf), u);
            x2 = Lerp(Gradient(aba, xf, yf - 1, zi), Gradient(bba, xf - 1, yf - 1, zf), u);
            y1 = Lerp(x1, x2, v);

            x1 = Lerp(Gradient(aab, xf, yf, zi - 1), Gradient(bab, xf - 1, yf, zf - 1), u);
            x2 = Lerp(Gradient(abb, xf, yf - 1, zi - 1), Gradient(bbb, xf - 1, yf - 1, zf - 1), u);
            y2 = Lerp(x1, x2, v);

            return Lerp(y1, y2, w);
        }

        private int Inc(int i)
        {
            ++i;
            if (Repeat > 0) i %= Repeat;
            return i;
        }

        private int Hash(int x, int y, int z) =>
            P(P(P(x) + y) + z);

        private int P(int i) => p[i % p.Length];

        private static float Gradient(int hash, float x, float y, float z)
        {
            int h = hash & 15;                                    // Take the hashed value and take the first 4 bits of it (15 == 0b1111)
            float u = h < 8 /* 0b1000 */ ? x : y;                // If the most significant bit (MSB) of the hash is 0 then set u = x.  Otherwise y.

            float v;                                             // In Ken Perlin's original implementation this was another conditional operator (?:).  I
                                                                  // expanded it for readability.

            if (h < 4 /* 0b0100 */)                                // If the first and second significant bits are 0 set v = y
                v = y;
            else if (h == 12 /* 0b1100 */ || h == 14 /* 0b1110*/)  // If the first and second significant bits are 1 set v = x
                v = x;
            else                                                  // If the first and second significant bits are not equal (0/1, 1/0) set v = z
                v = z;

            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v); // Use the last 2 bits to decide if u and v are positive or negative.  Then return their addition.
        }

        private static float Fade(float v) => v * v * v * (v * (v * 6 - 15) + 10);

        private static float Lerp(float a, float b, float x) => a + x * (b - a);
    }
}
