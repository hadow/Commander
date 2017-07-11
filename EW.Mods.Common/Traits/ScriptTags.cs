using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ScriptTagsInfo : UsesInit<ScriptTagsInit>
    {
        object ITraitInfo.Create(ActorInitializer init) { return new ScriptTags(init, this); }
    }

    public class ScriptTags
    {
        readonly HashSet<string> tags = new HashSet<string>();

        public ScriptTags(ActorInitializer init,ScriptTagsInfo info)
        {
            if (init.Contains<ScriptTagsInit>())
            {
                foreach (var tag in init.Get<ScriptTagsInit, string[]>())
                    tags.Add(tag);
            }
        }

        public bool AddTag(string tag)
        {
            return tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            return tags.Remove(tag);
        }

        public bool HasTag(string tag)
        {
            return tags.Contains(tag);
        }
    }

    public class ScriptTagsInit : IActorInit<string[]>
    {
        
        readonly string[] value = new string[0];

        public ScriptTagsInit() { }

        public ScriptTagsInit(string[] init) { value = init; }

        public string[] Value(World world) { return value; }

    }
}