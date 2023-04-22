using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public class WeightedSelector<TValue> : ISelector<TValue>
    {
        private readonly IDictionary<TValue, int> _weights;
        private readonly IList<(int, TValue)> _values;

        private WeightedSelector(Factory factory, Random random, IDictionary<TValue, int> weights)
        {
            SourceFactory = factory;
            Random = random;
            _weights = new Dictionary<TValue, int>(weights.Where(x => x.Value > 0));
            TotalWeight = 0;
            _values = new List<(int, TValue)>();
            foreach (var (value, weight) in _weights)
            {
                TotalWeight += weight;
                _values.Add((TotalWeight, value));
            }
        }

        public TValue this[int index] => _values[index].Item2;

        public Random Random { get; }

        public ISelector<TValue>.IFactory SourceFactory { get; }

        public int Outcomes => _values.Count;

        public int TotalWeight { get; }

        public IEnumerable<TValue> Values => _values.Select(x => x.Item2);

        public TValue Select()
        {
            int index = Random.Next(TotalWeight);
            foreach (var (maxWeight, value) in _values)
            {
                if (index < maxWeight) return value;
            }
            throw new Exception("Random generated a weight above the total weight");
        }

        public class Factory : ISelector<TValue>.IFactory
        {
            private static Random BASE_RANDOM_FACTORY() => new();
            private readonly IDictionary<TValue, int> _weights;

            public Random SharedRandom { get; set; }
            public Func<Random> RandomFactory { get; set; }
            public IEnumerable<TValue> Values => _weights.Keys;
            public int this[TValue option]
            {
                get
                {
                    if (_weights.TryGetValue(option, out var weight)) return weight;
                    return 0;
                }
                set
                {
                    if (value <= 0)
                    {
                        _weights.Remove(option);
                        return;
                    }
                    _weights[option] = value;
                }
            }

            public ISelector<TValue> Create() =>
                new WeightedSelector<TValue>(this, SharedRandom ?? (RandomFactory ?? BASE_RANDOM_FACTORY)(), _weights);
        }
    }
}
