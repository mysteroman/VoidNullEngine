using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Utils;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Rendering.Models
{
    public readonly struct Model
    {
        #region Presets
        private static readonly ConcurrentDictionary<ModelOrigin.PresetType, Model> presets = new();
        private static readonly ConcurrentDictionary<Vector2, Model> customs = new();

        public static Model Default => GetGeneric(default);
        #endregion

        private Model(uint id) => ModelID = id;

        public uint ModelID { get; }

        public static Model GetGeneric(ModelOrigin origin)
        {
            Model GetOrGenerate<TKey>(TKey key, ConcurrentDictionary<TKey, Model> registry)
            {
                var (x, y) = origin;
                return registry.GetOrAdd(key, _ => GenerateOffCenter(x, y));
            }

            return origin.IsCustom ? GetOrGenerate(origin.Origin, customs) : GetOrGenerate(origin.Preset, presets);
        }

        private static Model GenerateOffCenter(float originX, float originY)
        {
            originX /= 2;
            originY /= 2;

            float[] vertices =
            {
                -0.5f + originX, 0.5f + originY, 0f, 1f,    // top left
                0.5f + originX, 0.5f + originY, 1f, 1f,     // top right
                -0.5f + originX, -0.5f + originY, 0f, 0f,   // bottom left

                0.5f + originX, 0.5f + originY, 1f, 1f,     // top right
                0.5f + originX, -0.5f + originY, 1f, 0f,    // bottom right
                -0.5f + originX, -0.5f + originY, 0f, 0f,   // bottom left
            };

            return Generate(vertices);
        }

        public static Model CreateCustom(ModelData data) =>
            Generate(data.AsSpan());

        private static unsafe Model Generate(ReadOnlySpan<float> vertices)
        {
            uint id = glGenVertexArray();
            uint vbo = glGenBuffer();

            glBindVertexArray(id);
            glBindBuffer(GL_ARRAY_BUFFER, vbo);

            fixed (float* v = vertices)
            {
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_STATIC_DRAW);
            }

            glVertexAttribPointer(0, 2, GL_FLOAT, false, Vertex.Size * sizeof(float), (void*)0);
            glEnableVertexAttribArray(0);

            glVertexAttribPointer(1, 2, GL_FLOAT, false, Vertex.Size * sizeof(float), (void*)(2 * sizeof(float)));
            glEnableVertexAttribArray(1);

            glBindBuffer(GL_ARRAY_BUFFER, 0);
            glBindVertexArray(0);

            return new(id);
        }
    }
}
