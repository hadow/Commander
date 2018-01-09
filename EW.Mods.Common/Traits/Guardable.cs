using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// This unit can be guarded (followed and protected ) by a Guard unit
    /// </summary>
    public class GuardableInfo : TraitInfo<Guardable>
    {
        public readonly WDist Range = WDist.FromCells(2);//Maximum range that guarding actors will maintain.
    }
    public class Guardable { }
}