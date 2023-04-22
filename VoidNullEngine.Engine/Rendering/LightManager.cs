using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering.Objects;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering
{
    internal class LightManager
    {
        public const int MAX_LIGHT_SOURCES = 64;

        private const int DATA_SIZE = 7204;
        private const string BLOCK_NAME = "Light";
        private const uint BINDING = 1;
        
        private uint _buffer;
        private Vector3 _ambient;

        public LightManager()
        {
            AmbientLight = Vector3.One;
            PointLights = new List<PointLight>();
            DirectionalLights = new List<DirectionalLight>();
        }

        public ref Vector3 AmbientLight => ref _ambient;
        public List<PointLight> PointLights { get; }
        public List<DirectionalLight> DirectionalLights { get; }

        public void Load()
        {
            _buffer = glGenBuffer();
            glBindBuffer(GL_UNIFORM_BUFFER, _buffer);
            glBufferData(GL_UNIFORM_BUFFER, DATA_SIZE, IntPtr.Zero, GL_STATIC_DRAW);
            glBindBuffer(GL_UNIFORM_BUFFER, 0);

            glBindBufferBase(GL_UNIFORM_BUFFER, BINDING, _buffer);
        }

        public static void BindShader(uint shader)
        {
            uint index = glGetUniformBlockIndex(shader, BLOCK_NAME);
            glUniformBlockBinding(shader, index, BINDING);
        }

        public void Update()
        {
            glBindBuffer(GL_UNIFORM_BUFFER, _buffer);
            BufferAmbientLight();
            BufferPointLights();
            BufferDirectionalLights();
            glBindBuffer(GL_UNIFORM_BUFFER, 0);
        }

        private unsafe void BufferAmbientLight()
        {
            float[] ambient =
            {
                AmbientLight.X,
                AmbientLight.Y,
                AmbientLight.Z
            };

            fixed (float* value = ambient)
            {
                glBufferSubData(GL_UNIFORM_BUFFER, 0, 12, value);
            }
        }

        private unsafe void BufferPointLights()
        {
            int offset = 16;
            int n = 0;
            foreach (PointLight point in PointLights)
            {
                if (n >= MAX_LIGHT_SOURCES) break;
                
                float[] pos = { point.Position.X, point.Position.Y };
                fixed (float* p = pos)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset, 8, p);
                }

                float[] ambient = { point.AmbientLight.X, point.AmbientLight.Y, point.AmbientLight.Z };
                fixed (float* a = ambient)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset + 8, 12, a);
                }

                float[] light = { point.Light.X, point.Light.Y, point.Light.Z, point.Light.W };
                fixed (float* l = light)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset + 32, 16, l);
                }

                ++n;
                offset += 48;
            }
            glBufferSubData(GL_UNIFORM_BUFFER, 3088, 4, &n);
            PointLights.Clear();
        }

        private unsafe void BufferDirectionalLights()
        {
            int offset = 3104;
            int n = 0;
            foreach (DirectionalLight dir in DirectionalLights)
            {
                if (n >= MAX_LIGHT_SOURCES) break;

                float[] pos = { dir.Position.X, dir.Position.Y };
                fixed (float* p = pos)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset, 8, p);
                }

                float[] direction = { dir.Direction.X, dir.Direction.Y };
                fixed (float* d = direction)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset + 8, 8, d);
                }

                float focus = dir.Focus;
                glBufferSubData(GL_UNIFORM_BUFFER, offset + 16, 4, &focus);

                float[] ambient = { dir.AmbientLight.X, dir.AmbientLight.Y, dir.AmbientLight.Z };
                fixed (float* a = ambient)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset + 32, 12, a);
                }

                float[] light = { dir.Light.X, dir.Light.Y, dir.Light.Z, dir.Light.W };
                fixed (float* l = light)
                {
                    glBufferSubData(GL_UNIFORM_BUFFER, offset + 48, 16, l);
                }

                ++n;
                offset += 64;
            }
            glBufferSubData(GL_UNIFORM_BUFFER, 7200, 4, &n);
            DirectionalLights.Clear();
        }
    }
}
