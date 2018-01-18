using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// How much the unit is worth.
    /// </summary>
    public class ValuedInfo : TraitInfo<Valued>
    {
        /// <summary>
        /// The cost.
        /// </summary>
        [FieldLoader.Require]
        public readonly int Cost = 0;
    }
    public class Valued
    {
    }
}