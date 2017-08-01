using System;
using System.Collections.Generic;


namespace EW.Traits
{
    public class FactionInfo : TraitInfo<Faction>
    {
        public readonly string Name = null;

        public readonly string InternalName = null;

        public readonly HashSet<string> RandomFactionMembers = new HashSet<string>();

        public readonly string Side = null;

        [Translate]
        public readonly string Description = null;

        public readonly bool Selectable = true;
    }
    public class Faction
    {
    }
}