using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Core
{
    public sealed class Scene : IObjectTree
    {
        #region Management
        public static Scene Current { get; private set; } = new();
        #endregion
        #region Hierarchy
        private readonly ConcurrentDictionary<Guid, GameObject> hierarchy = new();
        private readonly List<GameObject> children = new();
        
        public int ChildCount => children.Count;
        public IEnumerable<GameObject> Children
        {
            get
            {
                lock (children) return new List<GameObject>(children);
            }
        }

        public GameObject FindChild(string name, bool direct)
        {
            lock (children)
            {
                if (string.IsNullOrEmpty(name)) return null;
                List<GameObject> current = children;
                do
                {
                    List<GameObject> next = new();
                    foreach (var child in current)
                    {
                        if (child.Name == name) return child;
                        if (!direct) next.AddRange(child.Children);
                    }
                    current = next;
                } while (current.Count > 0);
                return null;
            }
        }
        public GameObject FindChild(Guid id) => hierarchy.TryGetValue(id, out var obj) ? obj : null;
        public IList<GameObject> FindChildren(string name, bool direct)
        {
            lock (children)
            {
                List<GameObject> result = new();
                if (string.IsNullOrEmpty(name)) return result;
                List<GameObject> current = children;
                do
                {
                    List<GameObject> next = new();
                    foreach (var child in current)
                    {
                        if (child.Name == name) result.Add(child);
                        if (!direct) next.AddRange(child.Children);
                    }
                    current = next;
                } while (current.Count > 0);
                return result;
            }
        }

        public bool IsObjectInScene(GameObject obj) => obj is not null && hierarchy.ContainsKey(obj.Id);

        internal void AddChild(GameObject child)
        {
            lock (children)
            {
                lock (child.children)
                {
                    foreach (var (id, obj) in child.hierarchy)
                        hierarchy.TryAdd(id, obj);
                }
                children.Add(child);
            }
        }

        internal void RemoveChild(GameObject child)
        {
            lock (children)
            {
                lock (child.children)
                {
                    foreach (var pair in child.hierarchy)
                        hierarchy.TryRemove(pair);
                }
                children.Remove(child);
            }
        }
        #endregion
    }
}
