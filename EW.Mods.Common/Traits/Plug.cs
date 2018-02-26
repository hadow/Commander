using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PlugInfo:TraitInfo<Plug>
    {
        [FieldLoader.Require]
        public readonly string Type = null;
    }

    public class Plug { }
}