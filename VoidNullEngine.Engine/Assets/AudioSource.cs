using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Assets
{
    public sealed class AudioSource
    {
        private readonly string fileName;
        private readonly ConcurrentDictionary<uint, Clip> clips;
        private AudioSettings settings;
        private uint clipId;

        private AudioSource(string fileName)
        {
            this.fileName = fileName;
            clips = new ConcurrentDictionary<uint, Clip>();
            clipId = 0;
            settings = new AudioSettings();
            DefaultChannel = AudioChannel.Master;
        }

        internal static AudioSource Load(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            return new AudioSource(fileName);
        }

        public ref AudioSettings Settings => ref settings;
        public AudioChannel DefaultChannel { get; set; }

        public Clip Play(AudioSettings settings = default, AudioChannel channel = null) =>
            Play(false, settings, channel);
        public Clip PlayLooping(AudioSettings settings = default, AudioChannel channel = null) =>
            Play(true, settings, channel);
        public void PlaySync(AudioSettings settings = default, AudioChannel channel = null) =>
            Play(false, settings, channel).Wait();

        private Clip Play(bool loop, AudioSettings settings, AudioChannel channel)
        {
            var clip = new Clip(this, loop, settings, channel ?? DefaultChannel ?? AudioChannel.Master);
            clip.Play();
            return clip;
        }

        #region Instance Data

        public sealed class Clip
        {
            public readonly AudioSource Source;
            public readonly AudioChannel Channel;
            public readonly uint Id;

            private readonly string name;
            private readonly TaskCompletionSource tcs;
            private readonly bool looping;
            private AudioSettings settings;
            private bool playing;

            internal Clip(AudioSource source, bool loop, AudioSettings settings, AudioChannel channel)
            {
                Source = source;
                Channel = channel;
                looping = loop;
                this.settings = settings;

                Id = Interlocked.Increment(ref Source.clipId);
                name = $"\"{Source.fileName}{Id}\"";

                tcs = new TaskCompletionSource();
                playing = false;
            }

            public bool IsPlaying => playing;
            public bool ShouldLoop => looping;
            public ref AudioSettings Settings => ref settings;

            public TaskAwaiter GetAwaiter() => tcs.Task.GetAwaiter();
            public void Wait() => tcs.Task.Wait();

            public void Stop()
            {
                lock (tcs) playing = false;
            }

            internal void Play() => Task.Run(PlaySync);

            private void PlaySync()
            {
                if (!Open()) return;

                int length = GetLength();
                var oldSettings = GetSettings();
                ApplySettings(oldSettings);

                PlayFromStart();
                for (; ; )
                {
                    lock (tcs)
                    {
                        if (!playing) break;

                        int pos = GetPosition();
                        if (pos >= length)
                        {
                            if (!looping)
                            {
                                playing = false;
                                break;
                            }
                            PlayFromStart();
                        }

                        var newSettings = GetSettings();
                        if (newSettings != oldSettings) ApplySettings(oldSettings = newSettings);
                    }
                }

                Close();
                tcs.SetResult();
            }

            private bool Open()
            {
                if (SendString($"open \"{Source.fileName}\" type AVIVideo alias {name}"))
                {
                    Source.clips.TryAdd(Id, this);
                    return true;
                }
                return false;
            }

            private void PlayFromStart()
            {
                lock (tcs)
                {
                    playing = SendString($"play {name} from 0");
                }
            }

            private int GetLength()
            {
                if (SendString($"status {name} length", 255, out var sb))
                    return Convert.ToInt32(sb.ToString());
                return 0;
            }

            private int GetPosition()
            {
                if (SendString($"status {name} position", 255, out var sb))
                    return Convert.ToInt32(sb.ToString());
                return 0;
            }

            private void GetStatus(string value)
            {
                if (SendString($"status {name} {value}", 255, out var sb))
                    Debug.WriteLine(sb);
            }

            private AudioSettings GetSettings() =>
                Settings * Source.Settings * Channel.ComputeSettings();

            private void ApplySettings(AudioSettings settings)
            {
                float l = 1 - System.Math.Clamp(-settings.Pan, 0, 1);
                long leftVolume = (long)(l * 1000);
                SendString($"setaudio {name} left volume to {leftVolume} wait");

                float r = 1 - System.Math.Clamp(settings.Pan, 0, 1);
                long rightVolume = (long)(r * 1000);
                SendString($"setaudio {name} right volume to {rightVolume} wait");

                long volume = (long)(settings.Volume * 1000);
                SendString($"setaudio {name} volume to {volume} wait");
            }

            private void Close()
            {
                SendString($"stop {name}");
                SendString($"close {name}");
                Source.clips.TryRemove(Id, out var _);
            }
        }

        #endregion

        #region Windows DLL Stuff

        private static bool SendString(string cmd)
        {
            int err = mciSendString(cmd, null, 0, IntPtr.Zero);
            if (err != 0)
            {
                Debug.WriteLine($"ERROR {err} ON MCI COMMAND {cmd}");
                return false;
            }
            return true;
        }

        private static bool SendString(string cmd, int bufferSize, out StringBuilder buffer)
        {
            buffer = new StringBuilder();
            int err = mciSendString(cmd, buffer, bufferSize, IntPtr.Zero);
            if (err != 0)
            {
                if (buffer.Length > 0) Debug.WriteLine($"Output: {buffer}");
                Debug.WriteLine($"ERROR {err} ON MCI COMMAND {cmd}");
                return false;
            }
            return true;
        }

        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
        private static extern int mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);

        #endregion
    }
}
