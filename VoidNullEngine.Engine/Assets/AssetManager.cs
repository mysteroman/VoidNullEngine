using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Assets
{
    public static class AssetManager
    {
        private const int MAX_MEMORY_USAGE = 512000000;
        private static int _memoryUsage;
        private static readonly object _memoryLock;

        private static readonly SensitiveLoader<Texture> _textureLoader;
        private static readonly SensitiveLoader<Animation> _animationLoader;

        static AssetManager()
        {
            _memoryUsage = 0;
            _memoryLock = new object();

            Textures = new Registry<Texture>();
            _textureLoader = new SensitiveLoader<Texture>(Textures);

            Animations = new Registry<Animation>();
            _animationLoader = new SensitiveLoader<Animation>(Animations);

            Audio = new Registry<AudioSource>();
        }

        public static Registry<Animation> Animations { get; }
        public static Registry<Texture> Textures { get; }
        public static Registry<AudioSource> Audio { get; }

        public static void PreloadAnimation(string fileName) =>
            _animationLoader.Load(Animation.Load(fileName));

        public static void PreloadAnimation(string fileName, Vector2 spriteSize) =>
            _animationLoader.Load(Animation.Load(fileName, spriteSize));

        public static void PreloadTexture(string fileName) =>
            _textureLoader.Load(Texture.Load(fileName));

        public static void LoadAudio(string fileName) =>
            Audio[fileName] = AudioSource.Load(fileName);

        public static void LoadContent()
        {
            _textureLoader.LoadAll();
            _animationLoader.LoadAll();
        }

        private abstract class Loader<T>
        {
            public Loader(Registry<T> registry)
            {
                Registry = registry;
            }

            public Registry<T> Registry { get; }
            public abstract void LoadAll();
        }

        private class SensitiveLoader<T> : Loader<T>
        {
            private readonly List<Func<(int, Func<(string, T)>)>> _queue;
            private readonly ConcurrentQueue<Action> _loadingQueue;

            public SensitiveLoader(Registry<T> registry) : base(registry)
            {
                _queue = new List<Func<(int, Func<(string, T)>)>>();
                _loadingQueue = new ConcurrentQueue<Action>();
            }

            public override void LoadAll()
            {
                List<Task> tasks = new List<Task>();

                lock (_queue)
                {
                    foreach (var func in _queue)
                    {
                        var task = new Task(() =>
                        {
                            var loader = AwaitMemory(func);
                            _loadingQueue.Enqueue(() =>
                            {
                                var (fileName, result) = loader();
                                if (fileName is not null)
                                {
                                    Registry[fileName] = result;
                                    Debug.WriteLine($"LOADED {typeof(T).Name} {fileName}");
                                }
                            });
                        });

                        tasks.Add(task);
                    }
                    _queue.Clear();
                }

                foreach (var task in tasks) task.Start();

                Task.WaitAll(tasks.ToArray());
                while (_loadingQueue.TryDequeue(out var action)) action();
                Debug.WriteLine($"FINISHED LOADING {tasks.Count} {typeof(T).Name}(s)");
            }

            private static Func<(string, T)> AwaitMemory(Func<(int, Func<(string, T)>)> func)
            {
                for (; ; )
                {
                    lock (_memoryLock)
                    {
                        if (_memoryUsage < MAX_MEMORY_USAGE)
                        {
                            var (mem, @out) = func();
                            _memoryUsage += mem;
                            return @out;
                        }
                    }
                }
            }

            public void Load(Func<(int, Func<(string, T)>)> func)
            {
                lock (_queue) _queue.Add(func);
            }
        }

        private class InsensitiveLoader<T> : Loader<T>
        {
            private readonly List<Task> _queue;

            public InsensitiveLoader(Registry<T> registry) : base(registry)
            {
                _queue = new List<Task>();
            }

            public override void LoadAll()
            {
                Task[] tasks;
                lock (_queue)
                {
                    tasks = _queue.ToArray();
                    _queue.Clear();
                }

                Task.WaitAll(tasks);
                Debug.WriteLine($"FINISHED LOADING {tasks.Length} {typeof(T).Name}(s)");
            }

            public void Load(Task<(string, T)> task)
            {
                task.ContinueWith(tsk =>
                {
                    var (fileName, result) = tsk.Result;
                    if (fileName is not null)
                    {
                        Registry[fileName] = result;
                        Debug.WriteLine($"LOADED {typeof(T).Name} {fileName}");
                    }
                });

                lock (_queue) _queue.Add(task);
            }
        }

        public sealed class Registry<T>
        {
            private readonly Dictionary<string, T> _registry;

            internal Registry() =>
                _registry = new Dictionary<string, T>();

            public T this[string fileName]
            {
                get
                {
                    lock (this) return _registry.GetValueOrDefault(fileName);
                }
                internal set
                {
                    lock (this) _registry[fileName] = value;
                }
            }
        }
    }
}
