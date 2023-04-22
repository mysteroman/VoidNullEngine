using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Rendering.Objects.UniformBuffers;
using VoidNullEngine.Engine.Utils;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering
{
    public readonly struct Shader
    {
        private static Shader _global;
        public static ref Shader GlobalShader => ref _global;

        private uint ProgramID { get; }

        private Shader(uint id) => ProgramID = id;

        public static Shader Load(string vertexSource, string fragmentSource)
        {
            string vertex, fragment;
            try
            {
                vertex = File.ReadAllText(vertexSource);
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR READING VERTEX SHADER SOURCE CODE: " + e.Message);
                return default;
            }

            try
            {
                fragment = File.ReadAllText(fragmentSource);
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR READING FRAGMENT SHADER SOURCE CODE: " + e.Message);
                return default;
            }

            uint vs = CreateVertexShader(vertex);
            uint fs = CreateFragmentShader(fragment);

            uint id = glCreateProgram();
            glAttachShader(id, vs);
            glAttachShader(id, fs);
            glLinkProgram(id);

            glDetachShader(id, vs);
            glDetachShader(id, fs);
            glDeleteShader(vs);
            glDeleteShader(fs);

            LightManager.BindShader(id);

            return new Shader(id);
        }

        public void Use()
        {
            glUseProgram(ProgramID);
        }

        public void SetFloat(string uniformName, float value)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniform1f(uniform, value);
        }

        public void SetInt(string uniformName, int value)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniform1i(uniform, value);
        }

        public void SetVector2(string uniformName, Vector2 vector)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniform2f(uniform, vector.X, vector.Y);
        }

        public void SetVector3(string uniformName, Vector3 vector)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniform3f(uniform, vector.X, vector.Y, vector.Z);
        }

        public void SetVector4(string uniformName, Vector4 vector)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniform4f(uniform, vector.X, vector.Y, vector.Z, vector.W);
        }

        public void SetMatrix3x2(string uniformName, Matrix3x2 matrix)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniformMatrix3fv(uniform, 1, false, matrix.GetArray());
        }

        public void SetMatrix4x4(string uniformName, Matrix4x4 matrix)
        {
            int uniform = glGetUniformLocation(ProgramID, uniformName);
            glUniformMatrix4fv(uniform, 1, false, matrix.GetArray());
        }

        public void BindBuffer(string uniformName, IUniformBuffer value)
        {
            uint uniform = glGetUniformBlockIndex(ProgramID, uniformName);
            glUniformBlockBinding(ProgramID, uniform, value is not null ? value.Binding : 0);
        }

        private static uint CreateVertexShader(string vertex)
        {
            uint vs = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vs, vertex);
            glCompileShader(vs);

            int[] status = glGetShaderiv(vs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed
                string error = glGetShaderInfoLog(vs);
                Debug.WriteLine("ERROR COMPILING VERTEX SHADER: " + error);
            }

            return vs;
        }

        private static uint CreateFragmentShader(string fragment)
        {
            uint fs = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fs, fragment);
            glCompileShader(fs);

            int[] status = glGetShaderiv(fs, GL_COMPILE_STATUS, 1);
            if (status[0] == 0)
            {
                // Failed
                string error = glGetShaderInfoLog(fs);
                Debug.WriteLine("ERROR COMPILING FRAGMENT SHADER: " + error);
            }

            return fs;
        }
    }
}
