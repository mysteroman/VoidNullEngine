using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Net
{
    public readonly struct Frame : IDisposable
    {
        public readonly float DeltaTime;
        public readonly float TotalElapsedTime;
        private readonly MemoryStream GameState;


        
        public void Dispose()
        {
            if (GameState is not null)
                GameState.Dispose();
        }
    }
}
