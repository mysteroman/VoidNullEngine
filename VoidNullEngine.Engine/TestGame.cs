using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Entity;
using VoidNullEngine.Engine.Events;
using VoidNullEngine.Engine.Rendering;
using VoidNullEngine.Engine.Rendering.Objects;
using VoidNullEngine.Engine.Assets;
using VoidNullEngine.Engine.Utils;
using VoidNullEngine.Engine.Input;
using VoidNullEngine.Engine.Core;
using VoidNullEngine.Engine.Rendering.Models;

namespace VoidNullEngine.Engine
{
    class TestGame : Game
    {
        const string texture = @"res\textures\wall.jpg";
        const string anim1 = @"res\textures\Demopan powaaaaaah.gif";
        const string anim2 = @"res\textures\super.gif";
        const string audio = @"res\audio\music\event-horizon.wav";

        Element e1, e2, e3, e4, e5, e6;
        GameObject obj1, obj2, obj3, obj4, obj5, obj6;
        Model m;
        PointLight pLight;
        DirectionalLight dLight;
        CollisionManager border;
        AudioSource.Clip bgMusic;
        INoiseGenerator noise;

        public TestGame(int width, int height, string title) : base(width, height, title)
        {
        }

        protected override void Initialize()
        {
            noise = new PerlinNoiseGenerator();
        }

        protected override void LoadAssets()
        {
            AssetManager.PreloadTexture(texture);
            AssetManager.PreloadAnimation(anim1);
            AssetManager.PreloadAnimation(anim2);

            AssetManager.LoadContent();

            AssetManager.LoadAudio(audio);
            AssetManager.Audio[audio].DefaultChannel = AudioChannel.Music;

            Shader.GlobalShader = Shader.Load(@"res\shaders\vertexShader.glsl", @"res\shaders\fragmentShader.glsl");

            {
                var data = new ModelData();
                var t0 = data[0];

                var t1 = data.AddTriangle();
                {
                    var v0 = t1.V0;
                    v0.Y = -2f;
                    v0.TextureY = 1f;

                    var v1 = t1.V1;
                    v1.X = 1f;
                    v1.Y = -2f;
                    v1.TextureY = 1f;
                    v1.TextureX = 1f;

                    var v2 = t1.V2;
                    v2.Y = -3f;
                }

                Debug.WriteLine(data);

                {
                    var v0 = t0.V0;
                    v0.TextureY = 1f;

                    var v1 = t0.V1;
                    v1.X = 1f;
                    v1.TextureY = 1f;
                    v1.TextureX = 1f;

                    var v2 = t0.V2;
                    v2.Y = -1f;
                }

                Debug.WriteLine(data);

                m = Model.CreateCustom(data);
            }
        }

        protected override void LoadContent()
        {
            border = new CollisionManager(null);
            border.CollisionBoxes.Add(new CollisionBox
            {
                IsBoundingBox = true,
                TopLeft = Vector2.Zero,
                BottomRight = DisplayManager.WindowSize,
                Flags = 0
            });

            var a1 = AssetManager.Textures[texture];
            a1.PixelsPerUnit = 4;
            obj1 = new()
            {
                LocalPosition = new(200, 200)
            };
            e1 = new(obj1, m)
            {
                Layer = 0,
                Shader = Shader.GlobalShader,
                Texture = a1
            };

            var a2 = new Sprite(a1, new(0, 0, 256, 256));
            obj2 = new()
            {
                Parent = obj1,
                LocalPosition = new(96, -32)
            };
            e2 = new(obj2)
            {
                Layer = -0.1f,
                Shader = Shader.GlobalShader,
                Texture = a2
            };

            var a3 = new Sprite(a1, new(255, 0, 256, 256));
            obj3 = new()
            {
                Parent = obj1,
                LocalPosition = new(160, -32)
            };
            e3 = new(obj3)
            {
                Layer = -0.1f,
                Shader = Shader.GlobalShader,
                Texture = a3
            };

            var a4 = new Sprite(a1, new(0, 255, 256, 256));
            obj4 = new()
            {
                Parent = obj1,
                LocalPosition = new(96, 32)
            };
            e4 = new(obj4)
            {
                Layer = -0.1f,
                Shader = Shader.GlobalShader,
                Texture = a4
            };

            var a5 = new Sprite(a1, new(255, 255, 256, 256));
            obj5 = new()
            {
                Parent = obj1,
                LocalPosition = new(160, 32)
            };
            e5 = new(obj5)
            {
                Layer = -0.1f,
                Shader = Shader.GlobalShader,
                Texture = a5
            };

            var a6 = new Sprite(a5, new(127, 127, 128, 128));
            obj6 = new()
            {
                Parent = obj5,
                LocalPosition = new(16, 16)
            };
            e6 = new(obj6)
            {
                Layer = 0.1f,
                Shader = Shader.GlobalShader,
                Texture = a6
            };

            RenderManager.Camera = new Camera(DisplayManager.WindowSize / 2f);

            pLight = new PointLight
            {
                Position = new Vector2(200, 200),
                AmbientLight = Vector3.Zero,
                Light = new Vector4(1f, 1f, 1f, 50f)
            };

            dLight = new DirectionalLight
            {
                Position = new Vector2(200, 200),
                Direction = new Vector2(1f, 1f),
                Focus = 0.5f,
                AmbientLight = Vector3.Zero,
                Light = new Vector4(1f, 1f, 1f, 150f)
            };

            bgMusic = AssetManager.Audio[audio].Play();

            //for (int y = 0; y <= 50; y++)
            //{
            //    for (int x = 0; x <= 50; x++)
            //    {
            //        int z = (int)(255 * noise.GenerateNormal(x / 10f, y / 10f));
            //        Debug.Write(string.Format("{0:X2} ", z));
            //    }
            //    Debug.WriteLine(null);
            //}
        }

        protected override void Render()
        {
            RenderManager.ClearScreen();

            //RenderManager.PointLights.Add(pLight);
            //RenderManager.DirectionalLights.Add(dLight);

            RenderManager.RenderElement(e1);
            RenderManager.RenderElement(e2);
            RenderManager.RenderElement(e3);
            RenderManager.RenderElement(e4);
            RenderManager.RenderElement(e5);
            RenderManager.RenderElement(e6);

            //player.Render();

            RenderManager.RenderScene();
        }

        protected override void Update()
        {
            var inputs = LocalMouse.Instance.Query();
            // AudioChannel.Music.Settings.Pan = MathF.Cos(GameTime.TotalElapsedSeconds);
            bgMusic.Settings.Volume += inputs.Scroll.Y / 20;

            if (inputs.Mouse3)
            {
                AudioChannel.Music.Settings = default;
                AudioChannel.Master.Settings = default;
            }
        }
    }
}
