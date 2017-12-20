using System;
using System.Collections.Generic;


namespace EW.Traits
{
    /// <summary>
    /// Attach this to the 'World' actor.
    /// </summary>
    public class FactionInfo : TraitInfo<Faction>
    {
        /// <summary>
        /// This is the name exposed to the players.
        /// </summary>
        public readonly string Name = null;

        /// <summary>
        /// This is the internal name for owner checks.
        /// </summary>
        public readonly string InternalName = null;

        /// <summary>
        /// Picka random faction as the player's faction out of this list.
        /// </summary>
        public readonly HashSet<string> RandomFactionMembers = new HashSet<string>();

        /// <summary>
        /// The side that the faction belongs to.
        /// </summary>
        public readonly string Side = null;

        [Translate]
        public readonly string Description = null;

        public readonly bool Selectable = true;
    }
    public class Faction
    {
    }
}