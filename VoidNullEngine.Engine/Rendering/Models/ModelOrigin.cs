using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Rendering.Models
{
    public struct ModelOrigin
    {
        private PresetType preset;
        private Vector2 origin;

        public ModelOrigin(PresetType type)
        {
            origin = CENTER;
            preset = type;

            UpdatePivot(ref preset, ref origin);
        }

        public ModelOrigin(float xOffset, float yOffset)
        {
            if (!float.IsFinite(xOffset) || !float.IsFinite(yOffset))
                throw new ArgumentException($"Either {nameof(xOffset)} or {nameof(yOffset)} is not finite");
            origin = new(xOffset, yOffset);
            preset = GetPresetType(origin);
        }

        public ModelOrigin(Vector2 pivot)
        {
            if (!float.IsFinite(pivot.X) || !float.IsFinite(pivot.Y))
                throw new ArgumentException($"Either {nameof(pivot)}.{nameof(pivot.X)} or {nameof(pivot)}.{nameof(pivot.Y)} is not finite", nameof(pivot));
            origin = pivot;
            preset = GetPresetType(origin);
        }

        public PresetType Preset
        {
            readonly get => preset;
            set
            {
                if (preset == value) return;
                UpdatePivot(ref value, ref origin);
                preset = value;
            }
        }

        public Vector2 Origin
        {
            readonly get => origin;
            set
            {
                if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
                    throw new ArgumentException($"Either {nameof(value)}.{nameof(value.X)} or {nameof(value)}.{nameof(value.Y)} is not finite", nameof(value));
                if (origin == value) return;
                origin = value;
                if (preset != PresetType.Custom)
                    preset = GetPresetType(origin);
            }
        }

        public readonly bool IsCustom => preset == PresetType.Custom;

        public readonly void Deconstruct(out float xOffset, out float yOffset) => (xOffset, yOffset) = (origin.X, origin.Y);

        public enum PresetType
        {
            Center,
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left,
            Custom
        }

        private static readonly Vector2
            CENTER = Vector2.Zero,
            TOP_LEFT = new(-1, 1),
            TOP = new(0, 1),
            TOP_RIGHT = new(1, 1),
            RIGHT = new(1, 0),
            BOTTOM_RIGHT = new(1, -1),
            BOTTOM = new(0, -1),
            BOTTOM_LEFT = new(-1, -1),
            LEFT = new(-1, 0);

        private static PresetType GetPresetType(in Vector2 pivot)
        {
            if (pivot == CENTER) return PresetType.Center;
            if (pivot == TOP_LEFT) return PresetType.TopLeft;
            if (pivot == TOP) return PresetType.Top;
            if (pivot == TOP_RIGHT) return PresetType.TopRight;
            if (pivot == RIGHT) return PresetType.Right;
            if (pivot == BOTTOM_RIGHT) return PresetType.BottomRight;
            if (pivot == BOTTOM) return PresetType.Bottom;
            if (pivot == BOTTOM_LEFT) return PresetType.BottomLeft;
            if (pivot == LEFT) return PresetType.Left;
            return PresetType.Custom;
        }

        private static void UpdatePivot(ref PresetType origin, ref Vector2 pivot)
        {
            switch (origin)
            {
                case PresetType.Center:
                    pivot = CENTER;
                    break;
                case PresetType.TopLeft:
                    pivot = TOP_LEFT;
                    break;
                case PresetType.Top:
                    pivot = TOP;
                    break;
                case PresetType.TopRight:
                    pivot = TOP_RIGHT;
                    break;
                case PresetType.Right:
                    pivot = RIGHT;
                    break;
                case PresetType.BottomRight:
                    pivot = BOTTOM_RIGHT;
                    break;
                case PresetType.Bottom:
                    pivot = BOTTOM;
                    break;
                case PresetType.BottomLeft:
                    pivot = BOTTOM_LEFT;
                    break;
                case PresetType.Left:
                    pivot = LEFT;
                    break;
                case PresetType.Custom:
                    break;
                default:
                    origin = GetPresetType(pivot);
                    break;
            }
        }
    }
}
