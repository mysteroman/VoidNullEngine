using GLFW;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VoidNullEngine.Engine.Events;
using VoidNullEngine.Engine.Input;
using VoidNullEngine.Engine.Rendering;

namespace VoidNullEngine.Engine
{
    public abstract class Game
    {
        static Game()
        {
            
        }

        protected Game(int width, int height, string title)
        {
            InitialWindowWidth = width;
            InitialWindowHeight = height;
            InitialWindowTitle = title;
        }

        protected int InitialWindowWidth { get; }
        protected int InitialWindowHeight { get; }
        protected string InitialWindowTitle { get; }

        protected MouseInput Mouse { get; private set; }
        protected KeyboardInput Keyboard { get; private set; }

        public void Run()
        {
            Initialize();

            DisplayManager.CreateWindow(InitialWindowWidth, InitialWindowHeight, InitialWindowTitle);

            LocalKeyboard.Initialize();
            LocalMouse.Initialize();
            LocalController.Initialize();

            RenderManager.Load();

            LoadAssets();

            LoadContent();

            while (!Glfw.WindowShouldClose(DisplayManager.Window))
            {
                GameTime.Tick();

                EventManager.Send(new UpdateEvent());

                Update();
                
                EventManager.PollEvents();

                Render();

                Glfw.PollEvents();
            }

            DisplayManager.CloseWindow();
        }

        protected abstract void Initialize();
        protected abstract void LoadAssets();
        protected abstract void LoadContent();

        protected abstract void Update();
        protected abstract void Render();
    }
}
