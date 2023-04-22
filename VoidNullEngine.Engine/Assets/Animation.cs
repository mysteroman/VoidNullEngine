using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static VoidNullEngine.Engine.OpenGL.GL;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using VoidNullEngine.Engine.Rendering;

namespace VoidNullEngine.Engine.Assets
{
    public readonly struct Animation
    {
        public static readonly Animation None;
        private static readonly Func<(string, Animation)> NoneResult;

        static Animation()
        {
            None = new Animation();
            NoneResult = () => (null, None);
        }

        private Animation(string fileName, Vector2 size, Vector2 spriteSize, FrameData[] frames)
        {
            FileName = fileName;
            SpriteSize = spriteSize / (TextureSize = size);
            Duration = 0;
            foreach (FrameData frame in Frames = frames) Duration += frame.Duration;
        }

        private FrameData[] Frames { get; }

        public string FileName { get; }
        public Vector2 TextureSize { get; }
        public Vector2 SpriteSize { get; }
        public int FrameCount => Frames?.Length ?? 0;
        public float Duration { get; }

        public Frame FrameAt(Index frame) => Frames is null ? null : new Frame(this, frame);
        public Frame FrameAt(float time)
        {
            if (Frames is null) return null;
            if (time < 0) return new Frame(this, 0);
            time %= Duration;
            float totalDelta = 0;
            int index = 0;
            foreach (FrameData frame in Frames)
            {
                totalDelta += frame.Duration;
                if (time < totalDelta) return new Frame(this, index);
                ++index;
            }
            return new Frame(this, ^1);
        }

        private uint this[Index frame] => Frames?[frame].TextureID ?? 0;
        private uint this[float time]
        {
            get
            {
                if (Frames is null) return 0;
                if (time < 0) return Frames[0].TextureID;
                time %= Duration;
                float totalDelta = 0f;
                foreach (FrameData frame in Frames)
                {
                    totalDelta += frame.Duration;
                    if (time < totalDelta) return frame.TextureID;
                }
                return Frames[^1].TextureID;
            }
        }

        internal static Func<(int, Func<(string, Animation)>)> Load(string fileName, Vector2? spriteSize = null)
        {
            (int, Func<(string, Animation)>) Loader()
            {
                Debug.WriteLine($"START LOADING ANIMATION: {fileName}");

                using var image = Image.FromFile(fileName);

                if (!image.RawFormat.Equals(ImageFormat.Gif))
                {
                    Debug.WriteLine($"ERROR INVALID ANIMATION FILE FORMAT: {fileName}");
                    return (0, NoneResult);
                }

                Vector2 size = new Vector2(image.Width, image.Height);

                int frameCount = image.GetFrameCount(FrameDimension.Time);
                int memory = 0;
                (float, byte[])[] frames = new (float, byte[])[frameCount];

                const int FRAME_DELAY_ID = 0x5100;
                byte[] intervals = image.GetPropertyItem(FRAME_DELAY_ID).Value ?? Array.Empty<byte>();

                void LoadFrameData(int i)
                {
                    image.SelectActiveFrame(FrameDimension.Time, i);

                    float duration = 0f;
                    if (intervals.Length > 0)
                    {
                        int milliseconds = BitConverter.ToInt32(intervals, i * 4);
                        duration = milliseconds / 100f;
                    }

                    byte[] data;
                    using (var stream = new MemoryStream())
                    {
                        using (var bmp = new Bitmap(image))
                        {
                            bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                            bmp.Save(stream, ImageFormat.Bmp);
                        }
                        stream.Seek(0, SeekOrigin.Begin);

                        data = new byte[stream.Length - 1];
                        memory += data.Length;
                        for (int j = data.Length; j > 0;)
                        {
                            data[--j] = (byte)stream.ReadByte();
                        }
                    }

                    frames[i] = (duration, data);
                }

                for (int i = 0; i < frameCount; ++i) LoadFrameData(i);

                return (memory, () => Generate(fileName, size, spriteSize ?? size, frames));
            }
            if (!File.Exists(fileName))
            {
                Debug.WriteLine($"ERROR FINDING ANIMATION: {fileName}");
                return () => (0, NoneResult);
            }
            return Loader;
        }

        private static (string, Animation) Generate(string fileName, Vector2 size, Vector2 spriteSize, (float, byte[])[] frames)
        {
            unsafe FrameData GenerateFrame(int i)
            {
                var (duration, data) = frames[i];

                uint id = glGenTexture();
                glBindTexture(GL_TEXTURE_2D, id);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

                fixed (byte* image = data)
                {
                    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, (int)size.X, (int)size.Y, 0, GL_RGBA, GL_UNSIGNED_BYTE, image);
                    glGenerateMipmap(GL_TEXTURE_2D);
                }

                glBindTexture(GL_TEXTURE_2D, 0);
                return new FrameData(id, duration);
            }

            Debug.WriteLine($"LOADING ANIMATION {fileName} INTO OpenGL");

            FrameData[] actualFrames = new FrameData[frames.Length];
            for (int i = 0; i < frames.Length; ++i) actualFrames[i] = GenerateFrame(i);
            return (fileName, new Animation(fileName, size, spriteSize, actualFrames));
        }

        #region Inner Classes
        public abstract class AbstractInstance
        {
            protected AbstractInstance(Animation animation) =>
                Animation = animation;

            public Animation Animation { get; }
            public Vector2 SpriteSize => Animation.SpriteSize;
            public abstract uint TextureID { get; }
        }

        public sealed class Frame : AbstractInstance
        {
            public Frame(Animation animation, Index frameIndex) : base(animation) =>
                FrameIndex = frameIndex;

            public Index FrameIndex { get; }
            public override uint TextureID => Animation[FrameIndex];
        }

        public sealed class Instance : AbstractInstance
        {
            private static readonly Index FIRST_FRAME = 0;

            public float StartTime { get; private set; }
            public float? StopTime { get; private set; }

            public override uint TextureID =>
                float.IsNaN(StartTime) ? Animation[FIRST_FRAME] :
                StopTime is float stop ? Animation[stop - StartTime] :
                Animation[GameTime.TotalElapsedSeconds - StartTime];

            public Instance(Animation animation) : base(animation) =>
                (StartTime, StopTime) = (float.NaN, 0f);

            public void Restart() => (StartTime, StopTime) = (GameTime.TotalElapsedSeconds, null);

            public void Start()
            {
                if (float.IsNaN(StartTime)) StartTime = GameTime.TotalElapsedSeconds;
                StopTime = null;
            }

            public void Stop() => StopTime = GameTime.TotalElapsedSeconds;
        }

        private readonly struct FrameData
        {
            private const float DEFAULT_DURATION = 0.1f;

            public FrameData(uint id, float duration)
            {
                TextureID = id;
                Duration = duration == 0f ? DEFAULT_DURATION : duration;
            }

            public uint TextureID { get; }
            public float Duration { get; }
        }
        #endregion

        public static implicit operator Instance(Animation animation) => new Instance(animation);
    }
}
