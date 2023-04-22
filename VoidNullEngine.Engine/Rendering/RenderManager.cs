using GLFW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering.Objects;
using static VoidNullEngine.Engine.OpenGL.GL;
using VoidNullEngine.Engine.Assets;
using VoidNullEngine.Engine.Math;

namespace VoidNullEngine.Engine.Rendering
{
    public static class RenderManager
    {
        public const float ROTATION_0 = 0,
            ROTATION_45 = MathF.PI / 4,
            ROTATION_90 = MathF.PI / 2,
            ROTATION_135 = MathF.PI * 3 / 4,
            ROTATION_180 = MathF.PI,
            ROTATION_225 = MathF.PI * 5 / 4,
            ROTATION_270 = MathF.PI * 3 / 2,
            ROTATION_315 = MathF.PI * 7 / 4;

        private static readonly IComparer<IElement> _renderPriorityComparer = new PriorityComparer();
        private static readonly List<IElement> _elementQueue;
        private static readonly LightManager _lightManager;

        static RenderManager()
        {
            _elementQueue = new List<IElement>();
            _lightManager = new LightManager();
        }
        
        public static Camera Camera { get; set; }

        public static List<PointLight> PointLights => _lightManager.PointLights;
        public static List<DirectionalLight> DirectionalLights => _lightManager.DirectionalLights;
        public static ref Vector3 AmbientLight => ref _lightManager.AmbientLight;

        public static void Load()
        {
            _lightManager.Load();
        }

        public static void ClearScreen()
        {
            glClearColor(0, 0, 0, 1);
            glClear(GL_COLOR_BUFFER_BIT);
        }

        public static void RenderElement(Element e) =>
            _elementQueue.Add(e);

        public static void RenderScene()
        {
            _lightManager.Update();

            _elementQueue.Sort(_renderPriorityComparer);
            foreach (IElement e in _elementQueue) Render(e);
            _elementQueue.Clear();

            Glfw.SwapBuffers(DisplayManager.Window);
        }

        private static void Render(IElement e)
        {
            e.Shader.Use();
            e.Shader.SetMatrix4x4("model", e.Transform);
            e.Shader.SetMatrix4x4("projection", Camera.Projection);
            e.Shader.SetVector4("viewport", new(Camera.Viewport.MinX, Camera.Viewport.MinY, Camera.Viewport.MaxX, Camera.Viewport.MaxY));
            e.Shader.SetVector4("clearColor", new(0, 0, 0, 1));
            //e.Shader.SetVector3("ambientLight", Vector3.One);

            bool textured = e.Texture != null && e.Texture.TextureID != 0;
            e.Shader.SetInt("textured", textured ? 1 : 0);
            if (textured)
            {
                glActiveTexture(GL_TEXTURE0);
                glBindTexture(GL_TEXTURE_2D, e.Texture.TextureID);
                e.Shader.SetInt("texture.sample", 0);
                e.Shader.SetMatrix3x2("texture.transform", e.Texture.Transform);
            }

            glBindVertexArray(e.Model.ModelID);
            glDrawArrays(GL_TRIANGLES, 0, 6);
        }

        private class PriorityComparer : IComparer<IElement>
        {
            public int Compare(IElement x, IElement y) =>
                Compare(x.Priority, y.Priority);

            private static int Compare(Vector3 x, Vector3 y)
            {
                if (x.Z < y.Z) return -1;
                if (x.Z > y.Z) return 1;
                if (x.Y < y.Y) return -1;
                if (x.Y > y.Y) return 1;
                if (x.X < y.X) return -1;
                if (x.X > y.X) return 1;
                return 0;
            }
        }
    }
}
