using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Math;

namespace VoidNullEngine.Engine.Rendering
{
    public class Camera
    {
        private Matrix4x4? projection;
        private Vector2 focus;
        private float zoom;
        private Rect viewport = new(-1f, -1f, 2f, 2f);

        public Vector2 FocusPosition
        {
            get => focus;
            set
            {
                if (focus == value) return;
                focus = value;
                projection = null;
            }
        }
        public float Zoom
        {
            get => zoom;
            set
            {
                if (zoom == value) return;
                zoom = value;
                projection = null;
            }
        }
        public Rect Viewport
        {
            get => viewport;
            set
            {
                if (viewport == value) return;
                if (
                    viewport.MinX < -1f || viewport.MinY < -1f ||
                    viewport.MaxX <= -1f || viewport.MaxY <= -1f ||
                    viewport.MinX >= 1f || viewport.MinY >= 1f ||
                    viewport.MaxX > 1f || viewport.MaxY > 1f 
                    ) throw new ArgumentOutOfRangeException(nameof(value), $"Invalid viewport: {value}");
                viewport = value;
            }
        }

        public Matrix4x4 Projection
        {
            get
            {
                if (projection.HasValue) return projection.Value;

                float left = FocusPosition.X - DisplayManager.WindowSize.X / 2f;
                float right = FocusPosition.X + DisplayManager.WindowSize.X / 2f;
                float bottom = FocusPosition.Y + DisplayManager.WindowSize.Y / 2f;
                float top = FocusPosition.Y - DisplayManager.WindowSize.Y / 2f;

                Matrix4x4 orthoMatrix = Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, 0f, float.MaxValue);
                Matrix4x4 zoomMatrix = Matrix4x4.CreateScale(Zoom);

                Matrix4x4 result = orthoMatrix * zoomMatrix;
                projection = result;

                return result;
            }
        }

        public Camera(Vector2 focusPosition, float zoom = 1f)
        {
            FocusPosition = focusPosition;
            Zoom = zoom;
        }
    }
}
