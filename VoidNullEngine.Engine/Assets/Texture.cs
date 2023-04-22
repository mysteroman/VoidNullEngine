using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Math;
using static VoidNullEngine.Engine.OpenGL.GL;

namespace VoidNullEngine.Engine.Assets
{
    public class Texture : ITexture
    {
        private static readonly Func<(string, Texture)> NoneResult = () => (null, null);

        private float pxUnit = 1;

        public uint TextureID { get; }
        public string FileName { get; }
        public Vector2Int TextureSize { get; }
        public RectInt Bounds { get; }
        public Matrix3x2 Transform { get; }
        public float PixelsPerUnit
        {
            get => pxUnit;
            set
            {
                if (!float.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value), $"Pixel scale must be finite and above 0, is {value}");
                pxUnit = value;
            }
        }

        private Texture(uint textureID, string fileName, Vector2Int textureSize)
        {
            TextureID = textureID;
            FileName = fileName;
            Bounds = new(TextureSize = textureSize);
            Transform = Matrix3x2.Identity;
        }

        internal static Func<(int, Func<(string, Texture)>)> Load(string fileName)
        {
            (int, Func<(string, Texture)>) Loader()
            {
                Debug.WriteLine($"START LOADING IMAGE: {fileName}");

                using var image = Image.FromFile(fileName);

                bool alpha;
                switch (image.PixelFormat)
                {
                    case PixelFormat.Format32bppArgb:
                        alpha = true;
                        break;
                    case PixelFormat.Format24bppRgb:
                        alpha = false;
                        break;
                    default:
                        Debug.WriteLine($"ERROR INVALID IMAGE PIXEL FORMAT: {fileName}");
                        return (0, NoneResult);
                }

                Vector2Int size = new(image.Width, image.Height);
                byte[] data;
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, ImageFormat.Bmp);
                    data = stream.ToArray();
                }
                return (data.Length, () => Generate(fileName, size, alpha, data));
            }

            if (!File.Exists(fileName))
            {
                Debug.WriteLine($"ERROR FINDING IMAGE: {fileName}");
                return () => (0, NoneResult);
            }

            return Loader;
        }

        private unsafe static (string, Texture) Generate(string fileName, Vector2Int size, bool alpha, byte[] data)
        {
            Debug.WriteLine($"LOADING TEXTURE {fileName} INTO OpenGL");

            uint id = glGenTexture();
            glBindTexture(GL_TEXTURE_2D, id);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST_MIPMAP_LINEAR);

            fixed (byte* image = data)
            {
                glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, size.X, size.Y, 0, alpha ? GL_RGBA : GL_RGB, GL_UNSIGNED_BYTE, image);
                glGenerateMipmap(GL_TEXTURE_2D);
            }

            glBindTexture(GL_TEXTURE_2D, 0);
            return (fileName, new Texture(id, fileName, size));
        }
    }
}
