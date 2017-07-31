using System;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits.Render
{

    class ProductionBarInfo : ITraitInfo, Requires<ProductionInfo>
    {
        public object Create(ActorInitializer init) { return new ProductionBar(); }
    }
    class ProductionBar
    {
    }
}