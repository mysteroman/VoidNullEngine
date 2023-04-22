using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Core
{
    public interface IObjectTree
    {
        int ChildCount { get; }
        IEnumerable<GameObject> Children { get; }

        
        GameObject FindChild(string name, bool direct);
        GameObject FindChild(Guid id);
        IList<GameObject> FindChildren(string name, bool direct);
    }

    public static class IObjectTreeExtensions
    {
        public static GameObject FindChild(this IObjectTree tree, string name) => tree.FindChild(name, false);
        public static IList<GameObject> FindChildren(this IObjectTree tree, string name) => tree.FindChildren(name, false);
    }
}
