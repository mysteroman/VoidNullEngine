using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public interface ISelector<TValue>
    {
        IFactory SourceFactory { get; }
        Random Random { get; }
        TValue this[int index] { get; }
        int Outcomes { get; }
        IEnumerable<TValue> Values { get; }
        TValue Select();

        interface IFactory
        {
            Random SharedRandom { get; set; }
            Func<Random> RandomFactory { get; set; }
            
            ISelector<TValue> Create();
        }
    }

    public interface IConditionalSelector<TInput, TOutput> : ISelector<TOutput>
    {
        new IConditionalFactory SourceFactory { get; }
        IFactory ISelector<TOutput>.SourceFactory => SourceFactory;


        TOutput Select(TInput input);
        TOutput ISelector<TOutput>.Select() => Select(default);

        interface IConditionalFactory : IFactory
        {
            new IConditionalSelector<TInput, TOutput> Create();
            ISelector<TOutput> IFactory.Create() => Create();
        }
    }
}
