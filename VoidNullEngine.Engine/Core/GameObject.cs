using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Core
{
    [Serializable]
    public sealed class GameObject : IObjectTree, IEquatable<GameObject>
    {
        #region Transform
        private Vector2 localPosition, localScale;
        private Quaternion localRotation;

        public Vector2 LocalPosition
        {
            get => localPosition;
            set
            {
                if (localPosition == value) return;
                localPosition = value;
            }
        }
        public Vector2 Position
        {
            get => parent != null ? Vector2.Transform(localPosition, parent.Transform) : localPosition;
            set
            {
                if (parent == null)
                {
                    LocalPosition = value;
                    return;
                }

                Matrix4x4.Invert(parent.Transform, out var transform);
                LocalPosition = Vector2.Transform(value, transform);
            }
        }

        public Quaternion LocalRotation
        {
            get => localRotation;
            set
            {
                if (localRotation == value) return;
                localRotation = value;
            }
        }
        public Quaternion Rotation
        {
            get => parent != null ? localRotation * parent.Rotation : localRotation;
            set
            {
                if (parent == null)
                {
                    LocalRotation = value;
                    return;
                }

                LocalRotation = Quaternion.Inverse(parent.Rotation) * value;
            }
        }

        public Vector2 LocalScale
        {
            get => localScale;
            set
            {
                if (localScale == value) return;
                localScale = value;
            }
        }
        public Vector2 LossyScale =>  Vector2.Transform(localScale, LocalRotation) * (parent != null ? parent.LossyScale : Vector2.One);

        public Matrix4x4 Transform
        {
            get
            {
                Matrix4x4 trans = Matrix4x4.CreateTranslation(LocalPosition.X, LocalPosition.Y, 0);
                Matrix4x4 rot = Matrix4x4.CreateFromQuaternion(LocalRotation);
                Matrix4x4 scale = Matrix4x4.CreateScale(LocalScale.X, LocalScale.Y, 1);

                Matrix4x4 local = scale * rot * trans;
                if (parent != null) local *= parent.Transform;
                return local;
            }
        }
        #endregion
        #region Hierarchy
        internal readonly ConcurrentDictionary<Guid, GameObject> hierarchy = new();
        internal readonly List<GameObject> children = new();
        private GameObject parent;

        public GameObject Parent
        {
            get => parent;
            set
            {
                if (parent == value) return;

                if (parent != null) parent.RemoveChild(this);
                else Scene.Current.RemoveChild(this);

                if (value != null) value.AddChild(this);
                else Scene.Current.AddChild(this);

                parent = value;
            }
        }

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

        private void AddChild(GameObject child)
        {
            lock (children)
            {
                lock (child.children)
                {
                    for (var parent = this; parent != null; parent = parent.parent)
                        foreach (var (id, obj) in child.hierarchy)
                            parent.hierarchy.TryAdd(id, obj);
                }
                children.Add(child);
            }
        }

        private void RemoveChild(GameObject child)
        {
            lock (children)
            {
                lock (child.children)
                {
                    for (var parent = this; parent != null; parent = parent.parent)
                        foreach (var pair in child.hierarchy)
                            parent.hierarchy.TryRemove(pair);
                }
                children.Remove(child);
            }
        }
        #endregion
        public GameObject()
        {
            Id = Guid.NewGuid();
            name = GetDefaultName();
            localPosition = Vector2.Zero;
            localScale = Vector2.One;
            localRotation = Quaternion.Identity;
            Scene.Current.AddChild(this);
        }

        public readonly Guid Id;
        private string name;
        public string Name
        {
            get => name;
            set => name = string.IsNullOrEmpty(value) ? GetDefaultName() : value;
        }

        private string GetDefaultName()
        {
            const string BASE_NAME = nameof(GameObject);
            const string DUPES_PATTERN = @$"{BASE_NAME}(\s+\((?<N>[1-9]\d*)\))?";
            StringBuilder builder = new(BASE_NAME);
            if (parent != null)
            {
                Regex regex = new(DUPES_PATTERN);
                int n = -1;

                foreach (var sibling in parent.Children)
                {
                    Match match = regex.Match(sibling.name);
                    if (match.Success)
                        n = System.Math.Max(n, match.Groups.TryGetValue("N", out var group) && int.TryParse(group.Value, out var num) ? num : 0);
                }

                if (++n > 0) builder.Append($" ({n})");
            }
            return builder.ToString();
        }

        public bool Equals(GameObject other) =>
            other is not null && other.Id == Id;

        public override bool Equals(object obj) => obj is GameObject go && Equals(go);

        public override int GetHashCode() => HashCode.Combine(typeof(GameObject), Id);

        public override string ToString() => Name;

        public static bool operator ==(GameObject left, GameObject right) =>
            left is not null ? left.Equals(right) : right is null;
        public static bool operator !=(GameObject left, GameObject right) => !(left == right);
    }
}
