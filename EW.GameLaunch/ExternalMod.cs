using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
namespace EW
{
    public class ExternalMod
    {
        public readonly string Id;
        public readonly string Version;
        public readonly string Title;
        public readonly string LaunchPath;
        public readonly string[] LaunchArgs;
        public Sprite Icon { get; internal set; }

        public static string MakeKey(Manifest mod) { return MakeKey(mod.Id, mod.Metadata.Version); }
        public static string MakeKey(ExternalMod mod) { return MakeKey(mod.Id, mod.Version); }
        public static string MakeKey(string modId, string modVersion) { return modId + "-" + modVersion; }
    }

    public class ExternalMods : IReadOnlyDictionary<string, ExternalMod>
    {
        readonly Dictionary<string, ExternalMod> mods = new Dictionary<string, ExternalMod>();

        public ExternalMod this[string key] { get { return mods[key]; } }
        public int Count { get { return mods.Count; } }
        public ICollection<string> Keys { get { return mods.Keys; } }
        public ICollection<ExternalMod> Values { get { return mods.Values; } }
        public bool ContainsKey(string key) { return mods.ContainsKey(key); }
        public IEnumerator<KeyValuePair<string, ExternalMod>> GetEnumerator() { return mods.GetEnumerator(); }
        public bool TryGetValue(string key, out ExternalMod value) { return mods.TryGetValue(key, out value); }
        IEnumerator IEnumerable.GetEnumerator() { return mods.GetEnumerator(); }
    }
}