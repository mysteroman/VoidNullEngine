using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Assets
{
    public sealed class AudioChannel
    {
        public const string CH_MASTER = "MASTER";
        public const string CH_MUSIC = "music";
        public const string CH_SFX = "sfx";

        #region Instance Data

        public readonly AudioChannel Parent;
        public readonly string Name;
        public readonly string Namespace;

        public ref AudioSettings Settings => ref settings;

        private readonly ConcurrentDictionary<string, AudioChannel> subChannels;
        private AudioSettings settings;

        private AudioChannel(AudioChannel parent, string name)
        {
            Parent = parent;
            Namespace = Name = name;
            subChannels = new ConcurrentDictionary<string, AudioChannel>();
            if (Parent is not null)
            {
                Namespace = string.Join('.', Parent.Namespace, Name);
            }
        }

        public AudioSettings ComputeSettings() =>
            Parent is not null ?
            Parent.ComputeSettings() * Settings :
            Settings;

        public bool TryCreateSubChannel(string @namespace, out AudioChannel subChannel)
        {
            if (!ValidateNamespace(@namespace)) throw new ArgumentException($"Given namespace does not fit the required format");
            if (string.IsNullOrEmpty(@namespace)) throw new ArgumentException($"Namespace cannot be empty");

            string[] names = @namespace.Split('.');
            return TryCreateSubChannel(this, names, out subChannel);
        }

        public AudioChannel GetSubChannel(string @namespace)
        {
            if (!ValidateNamespace(@namespace)) throw new ArgumentException($"Given namespace does not fit the required format");
            if (string.IsNullOrEmpty(@namespace)) throw new ArgumentException($"Namespace cannot be empty");

            string[] names = @namespace.Split('.');
            return FindSubChannel(this, names);
        }

        private bool TryCreateDirectSubChannel(string name, out AudioChannel subChannel)
        {
            if (!subChannels.TryAdd(name, subChannel = new AudioChannel(this, name)))
            {
                subChannel = null;
                return false;
            }
            return true;
        }

        private bool TryGetDirectSubChannel(string name, out AudioChannel subChannel) =>
            subChannels.TryGetValue(name, out subChannel);

        private AudioChannel GetOrCreateDirectSubChannel(string name) =>
            subChannels.GetOrAdd(name, x => new AudioChannel(this, x));

        #endregion
        #region Class Data

        public static readonly AudioChannel Master;
        public static readonly AudioChannel Music;
        public static readonly AudioChannel Sfx;

        private static readonly Regex NAMESPACE_PATTERN;

        static AudioChannel()
        {
            Master = new AudioChannel(null, CH_MASTER);
            Master.TryCreateDirectSubChannel(CH_MUSIC, out Music);
            Master.TryCreateDirectSubChannel(CH_SFX, out Sfx);

            string NAME_REGEX = @"[a-z\-_]+";
            NAMESPACE_PATTERN = new Regex(@$"^({CH_MASTER})?(((?<!^)\.)?{NAME_REGEX})*$", RegexOptions.Compiled);
        }

        public static AudioChannel GetChannel(string @namespace)
        {
            if (!ValidateNamespace(@namespace)) throw new ArgumentException($"Given namespace does not fit the required format");
            if (string.IsNullOrEmpty(@namespace)) return Master;

            string[] names = @namespace.Split('.');
            if (names[0].Equals(CH_MASTER)) names = names[1..];
            return FindSubChannel(Master, names);
        }

        public static bool TryCreateChannel(string @namespace, out AudioChannel channel)
        {
            if (!ValidateNamespace(@namespace)) throw new ArgumentException($"Given namespace does not fit the required format");
            if (string.IsNullOrEmpty(@namespace)) throw new ArgumentException($"Namespace cannot be empty");

            string[] names = @namespace.Split('.');

            if (names[0].Equals(CH_MASTER) && names.Length > 1) names = names[1..];
            return TryCreateSubChannel(Master, names, out channel);
        }

        public static bool TryCreateChannel(string @namespace) =>
            TryCreateChannel(@namespace, out var _);

        private static bool ValidateNamespace(string @namespace) =>
            NAMESPACE_PATTERN.IsMatch(@namespace);

        private static AudioChannel FindSubChannel(AudioChannel parent, string[] names)
        {
            foreach (string name in names)
            {
                if (!parent.TryGetDirectSubChannel(name, out parent)) break;
            }
            return parent;
        }

        private static bool TryCreateSubChannel(AudioChannel parent, string[] names, out AudioChannel channel)
        {
            string lastName = names[^1];
            names = names[..^1];
            foreach (string name in names) parent = parent.GetOrCreateDirectSubChannel(name);
            return parent.TryCreateDirectSubChannel(lastName, out channel);
        }

        #endregion
    }
}
