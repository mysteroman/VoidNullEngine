using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Core;

namespace VoidNullEngine.Engine.Assets.Animations
{
    public struct KeyFrame
    {
        private ConcurrentDictionary<AnimationProperty, object> properties;
        private int length;

        public int Length
        {
            readonly get => length;
            set
            {
                if (length == value) return;
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(KeyFrame)} {Length} must be at least 1");
                length = value;
            }
        }

        public ConcurrentDictionary<AnimationProperty, object> Properties => properties ??= new();
    }
}
