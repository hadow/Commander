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

        public static object LoadVersus(MiniYaml yaml)
        {
            var nd = yaml.ToDictionary();
            return nd.ContainsKey("Versus") ? nd["Versus"].ToDictionary(my => FieldLoader.GetValue<int>("(value)", my.Value)) : new Dictionary<string, int>();
        }

    }
}