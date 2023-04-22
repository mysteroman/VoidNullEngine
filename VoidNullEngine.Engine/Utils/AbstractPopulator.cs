using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public abstract class AbstractPopulator
    {
        protected readonly Random random;

        public AbstractPopulator(int seed) => random = new(seed);

        public abstract ICollection<Vector2> Populate(Vector2 area, int n);
        public abstract ICollection<Vector2> Populate(Vector2 area, double minDistance);
    }
}
