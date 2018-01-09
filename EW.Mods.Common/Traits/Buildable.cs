using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class BuildableInfo : TraitInfo<Buildable>
    {

        public readonly HashSet<string> Queue = new HashSet<string>();

        /// <summary>
        /// Disable production when there are more than this many of this actor on the battlefield.
        /// </summary>
        public readonly int BuildLimit = 0;

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
    public class Buildable
    {
    }
}