using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public class UniformSelector<TValue> : ISelector<TValue>
    {
        private readonly IList<TValue> _values;

        private UniformSelector(Factory factory, Random random, IEnumerable<TValue> values)
        {
            SourceFactory = factory;
            Random = random;
            _values = new List<TValue>(values);
        }
        
        public TValue this[int index] => _values[index];

        public Random Random { get; }

        public ISelector<TValue>.IFactory SourceFactory { get; }

        public int Outcomes => _values.Count;

        public IEnumerable<TValue> Values => _values.Select(x => x);

        public TValue Select() => this[Random.Next(Outcomes)];

        public class Factory : ISelector<TValue>.IFactory
        {
            private static Random BASE_RANDOM_FACTORY() => new();

            public Random SharedRandom { get; set; }
            public Func<Random> RandomFactory { get; set; }
            public ICollection<TValue> Values { get; } = new List<TValue>();

            public ISelector<TValue> Create() =>
                new UniformSelector<TValue>(this, SharedRandom ?? (RandomFactory ?? BASE_RANDOM_FACTORY)(), Values);
        }
    }
}
