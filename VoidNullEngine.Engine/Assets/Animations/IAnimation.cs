using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Core;
using VoidNullEngine.Engine.Rendering.Objects;

namespace VoidNullEngine.Engine.Assets.Animations
{
    public interface IAnimation
    {
        KeyFrame this[Index frameIndex] { get; set; }
        AnimationCurve this[AnimationProperty property, Index tick] { get; set; }

        bool Looping { get; }
        int FrameCount { get; }
        int TickLength { get; }
        float TickInterval { get; }
        float TotalDuration { get; }
    }
}
