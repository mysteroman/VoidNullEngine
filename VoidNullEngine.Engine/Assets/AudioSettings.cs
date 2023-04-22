using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Assets
{
    public struct AudioSettings : IEquatable<AudioSettings>
    {
        private float? volume;
        private float? pan;

        public float Volume
        {
            get => volume ?? 1f;
            set => volume = System.Math.Clamp(value, 0f, 1f);
        }

        public float Pan
        {
            get => pan ?? 0f;
            set => pan = System.Math.Clamp(value, -1f, 1f);
        }

        public void Reset()
        {
            volume = null;
            pan = null;
        }

        public bool Equals(AudioSettings other) =>
            Volume == other.Volume &&
            Pan == other.Pan;

        public override bool Equals(object obj) =>
            obj is AudioSettings settings && Equals(settings);

        public override int GetHashCode() =>
            HashCode.Combine(Volume, Pan);

        public static AudioSettings operator *(AudioSettings a, AudioSettings b) => new AudioSettings
        {
            Volume = a.Volume * b.Volume,
            Pan = a.Pan + b.Pan
        };

        public static bool operator ==(AudioSettings left, AudioSettings right) =>
            left.Equals(right);

        public static bool operator !=(AudioSettings left, AudioSettings right) =>
            !left.Equals(right);
    }
}
