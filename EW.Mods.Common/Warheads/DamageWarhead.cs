using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Warheads
{
    public abstract class DamageWarhead:Warhead
    {
        public readonly int Damage = 0;

        /// <summary>
        /// Types of damage that this warhead causes
        /// </summary>
        public readonly HashSet<string> DamageTypes = new HashSet<string>();

        [FieldLoader.LoadUsing("LoadVersus")]
        public readonly Dictionary<string, int> Versus;



    }
}