using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public class BooleanSelector : ISelector<bool>
    {
        private const double BASE_CHANCE = .5;

        public BooleanSelector(Random random, double chance)
        {
            if (!double.IsFinite(chance)) throw new ArgumentException($"Must be a finite number, is {chance}", nameof(chance));
            Random = random ?? throw new ArgumentNullException(nameof(random));
            Chance = chance;
        }
        public BooleanSelector(double chance) : this(new(), chance) { }
        public BooleanSelector(Random random) : this(random, BASE_CHANCE) { }
        public BooleanSelector() : this(new(), BASE_CHANCE) { }

        private BooleanSelector(Factory factory, Random random, double chance)
        {
            SourceFactory = factory;
            Random = random;
            Chance = chance;
        }

        public bool this[int index] => 
            index != 0 && (index == 1 ? true : throw new ArgumentOutOfRangeException(nameof(index)));

        public Random Random { get; }
        public double Chance { get; }
        public ISelector<bool>.IFactory SourceFactory { get; }

        public int Outcomes => Chance > 0 && Chance < 1 ? 2 : 1;

        public IEnumerable<bool> Values
        {
            get
            {
                if (Chance < 1) yield return false;
                if (Chance > 0) yield return true;
            }
        }

        public bool Select() => Random.NextDouble() < Chance;

        public class Factory : ISelector<bool>.IFactory
        {
            private static Random BASE_RANDOM_FACTORY() => new();
            private double _chance = BASE_CHANCE;
            
            public Random SharedRandom { get; set; }
            public Func<Random> RandomFactory { get; set; }
            public double Chance
            {
                get => _chance;
                set => _chance = double.IsFinite(value) ? value : throw new ArgumentException($"Must be a finite number, is {value}", nameof(value));
            }

            public ISelector<bool> Create() =>
                new BooleanSelector(this, SharedRandom ?? (RandomFactory ?? BASE_RANDOM_FACTORY)(), Chance);
        }
    }
}
