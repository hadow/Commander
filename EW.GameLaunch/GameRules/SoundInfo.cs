using System;
using System.Collections.Generic;
using System.Linq;
namespace EW.GameRules
{
    public class SoundInfo
    {
        public readonly Dictionary<string,string[]> Variants = new Dictionary<string, string[]>();
        public readonly Dictionary<string,string[]> Prefixes = new Dictionary<string, string[]>();
        public readonly Dictionary<string,string[]> Voices = new Dictionary<string, string[]>();
        public readonly Dictionary<string, string[]> Notifications = new Dictionary<string, string[]>();

        public readonly string DefaultVariant = ".aud";
        public readonly string DefaultPrefix = "";

        public readonly HashSet<string> DisableVariants = new HashSet<string>();
        public readonly HashSet<string> DisablePrefixed = new HashSet<string>();

        public readonly Lazy<Dictionary<string, SoundPool>> VoicePools;
        public readonly Lazy<Dictionary<string, SoundPool>> NotificationsPools;
       



        public SoundInfo(MiniYaml y)
        {
            FieldLoader.Load(this, y);

            VoicePools = Exts.Lazy(() => Voices.ToDictionary(a => a.Key, a => new SoundPool(a.Value)));
            NotificationsPools = Exts.Lazy(() => Notifications.ToDictionary(a => a.Key, a => new SoundPool(a.Value)));
        }
    }

    public class SoundPool
    {
        readonly string[] clips;
        readonly List<string> liveclips = new List<string>();

        public SoundPool(params string[] clips)
        {
            this.clips = clips;
        }

        public string GetNext()
        {
            if (liveclips.Count == 0)
                liveclips.AddRange(clips);

            if (liveclips.Count == 0)
                return null;

            var i = WarGame.CosmeticRandom.Next(liveclips.Count);
            var s = liveclips[i];
            liveclips.RemoveAt(i);
            return s;
        }


    }
}