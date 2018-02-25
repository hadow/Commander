using System;
using EW.Traits;
using System.Collections.Generic;
namespace EW.Mods.Common.Traits
{
    public class ProvidesTechPrerequisiteInfo : ITechTreePrerequisiteInfo
    {
        [Desc("Internal id for this tech level.")]
        public readonly string Id;

        public readonly string Name;

        [Desc("Prerequisites to grant when this tech level is active.")]
        public readonly string[] Prerequisites = { };

        public object Create(ActorInitializer init) { return new ProvidesTechPrerequisite(this,init); }
    }
    public class ProvidesTechPrerequisite:ITechTreePrerequisite
    {

        readonly ProvidesTechPrerequisiteInfo info;
        bool enabled;

        static readonly string[] NoPrerequisites = new string[0];

        public string Name { get { return info.Name; } }

        public IEnumerable<string> ProvidesPrerequisites
        {
            get
            {
                return enabled ? info.Prerequisites : NoPrerequisites;
            }
        }

        public ProvidesTechPrerequisite(ProvidesTechPrerequisiteInfo info,ActorInitializer init)
        {
            this.info = info;
            var mapOptions = init.World.WorldActor.TraitOrDefault<MapOptions>();

            enabled = mapOptions != null && mapOptions.TechLevel == info.Id;
        }

    }
}