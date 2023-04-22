using System;
using System.Diagnostics;
using VoidNullEngine.Engine.Scripting;

namespace VoidNullEngine.Engine
{
    class Program
    {
        public static void Main(string[] args)
        {   
            Game game = new TestGame(800, 600, "Test Game");
            game.Run();
        }
    }
}
