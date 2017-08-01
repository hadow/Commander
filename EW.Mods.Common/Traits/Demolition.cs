using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class DemolitionInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Demolition(this); }
    }
    class Demolition
    {

        public Demolition(DemolitionInfo info) { }
    }
}