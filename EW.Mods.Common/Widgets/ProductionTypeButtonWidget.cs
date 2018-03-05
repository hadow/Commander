using System;
using EW.Widgets;
namespace EW.Mods.Common.Widgets
{
    public class ProductionTypeButtonWidget:ButtonWidget
    {
        public readonly string ProductionGroup;

        [ObjectCreator.UseCtor]
        public ProductionTypeButtonWidget(ModData modData)
            : base(modData) { }

        protected ProductionTypeButtonWidget(ProductionTypeButtonWidget other)
            : base(other)
        {
            ProductionGroup = other.ProductionGroup;
        }
    }
}
