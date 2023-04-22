using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Entity
{
    public readonly struct CollisionFlag: IEquatable<CollisionFlag>
    {
        private ulong Flags1 { get; init; }
        private ulong Flags2 { get; init; }
        private ulong Flags3 { get; init; }
        private ulong Flags4 { get; init; }

        public CollisionFlag(CollisionFlag copy) =>
            (Flags1, Flags2, Flags3, Flags4) = 
            (copy.Flags1, copy.Flags2, copy.Flags3, copy.Flags4);

        public bool this[byte order]
        {
            get
            {
                ulong bit = 1u << (order % 64);
                if (order < 64) return (bit & Flags1) > 0u;
                if (order < 128) return (bit & Flags2) > 0u;
                if (order < 192) return (bit & Flags3) > 0u;
                return (bit & Flags4) > 0u;
            }
            init
            {
                if (!value ^ this[order]) return;
                ulong bit = 1u << (order % 64);
                if (order < 64) Flags1 ^= bit;
                else if (order < 128) Flags2 ^= bit;
                else if (order < 192) Flags3 ^= bit;
                else Flags4 ^= bit;
            }
        }

        public override string ToString() =>
            string.Format("{0:X}{1:X}{2:X}{3:X}", Flags1, Flags2, Flags3, Flags4);

        public override bool Equals(object obj) =>
            obj is CollisionFlag flag && Equals(flag);

        public bool Equals(CollisionFlag flag) =>
                Flags1 == flag.Flags1 &&
                Flags2 == flag.Flags2 &&
                Flags3 == flag.Flags3 &&
                Flags4 == flag.Flags4;

        public override int GetHashCode() =>
            HashCode.Combine(Flags1, Flags2, Flags3, Flags4);

        public static implicit operator CollisionFlag(byte order) => new CollisionFlag
        {
            [order] = true
        };

        #region Logical operators

        public static bool operator ==(CollisionFlag left, CollisionFlag right) =>
            left.Equals(right);

        public static bool operator !=(CollisionFlag left, CollisionFlag right) =>
            !left.Equals(right);

        public static implicit operator bool(CollisionFlag flag) =>
            flag.Flags1 > 0u || flag.Flags2 > 0u || flag.Flags3 > 0u || flag.Flags4 > 0u;

        #endregion
        #region Bitwise operators

        public static CollisionFlag operator |(CollisionFlag flag1, CollisionFlag flag2) => new CollisionFlag
        {
            Flags1 = flag1.Flags1 | flag2.Flags1,
            Flags2 = flag1.Flags2 | flag2.Flags2,
            Flags3 = flag1.Flags3 | flag2.Flags3,
            Flags4 = flag1.Flags4 | flag2.Flags4
        };

        public static CollisionFlag operator |(CollisionFlag flag, byte order) => new CollisionFlag(flag)
        {
            [order] = true
        };

        public static CollisionFlag operator &(CollisionFlag flag1, CollisionFlag flag2) => new CollisionFlag
        {
            Flags1 = flag1.Flags1 & flag2.Flags1,
            Flags2 = flag1.Flags2 & flag2.Flags2,
            Flags3 = flag1.Flags3 & flag2.Flags3,
            Flags4 = flag1.Flags4 & flag2.Flags4
        };

        public static CollisionFlag operator &(CollisionFlag flag, byte order) => new CollisionFlag
        {
            [order] = flag[order]
        };

        public static CollisionFlag operator ^(CollisionFlag flag1, CollisionFlag flag2) => new CollisionFlag
        {
            Flags1 = flag1.Flags1 ^ flag2.Flags1,
            Flags2 = flag1.Flags2 ^ flag2.Flags2,
            Flags3 = flag1.Flags3 ^ flag2.Flags3,
            Flags4 = flag1.Flags4 ^ flag2.Flags4
        };

        public static CollisionFlag operator ^(CollisionFlag flag, byte order) => new CollisionFlag(flag)
        {
            [order] = flag[order] ^ true
        };

        #endregion
    }
}
