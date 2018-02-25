using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class BuildableInfo : TraitInfo<Buildable>
    {

        public readonly string[] Prerequisites = { };

        /// <summary>
        /// Production queue(s) that can produce this.
        /// </summary>
        public readonly HashSet<string> Queue = new HashSet<string>();

        /// <summary>
        /// Disable production when there are more than this many of this actor on the battlefield.
        /// </summary>
        public readonly int BuildLimit = 0;


        [Desc("Base build time in frames (-1 indicates to use the unit's value)")]
        public readonly int BuildDuration = -1;


        /// <summary>
        /// Percentage modifier to apply to the build duration.
        /// </summary>
        public readonly int BuildDurationModifier = 60;


        public readonly string ForceFaction = null;
        [SequenceReference]
        public readonly string Icon = "icon";

        [PaletteReference]
        public readonly string IconPalette = "chrome";

        [Translate]
        public readonly string Description = "";


        public static string GetInitialFaction(ActorInfo ai,string defaultFaction){

            var bi = ai.TraitInfoOrDefault<BuildableInfo>();

            return bi != null ? bi.ForceFaction ?? defaultFaction : defaultFaction;
        }
    }
    public class Buildable{}
}