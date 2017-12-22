using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class PluggableInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new Pluggable();
        }
    }

    public class Pluggable
    {

    }


    public class PlugsInit : IActorInit<Dictionary<CVec, string>>
    {
        [DictionaryFromYamlKey]
        readonly Dictionary<CVec, string> value = new Dictionary<CVec, string>();

        public PlugsInit() { }

        public PlugsInit(Dictionary<CVec,string> init)
        {
            value = init;
        }

        public Dictionary<CVec,string> Value(World world) { return value; }
    }
}