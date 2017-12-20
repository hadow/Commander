using System;
using EW.Traits;
using EW.Support;
namespace EW.Mods.Common.Traits
{
    public abstract class PausableConditionalTraitInfo:ConditionalTraitInfo
    {


    }


    public abstract class PausableConditionalTrait<InfoType>:ConditionalTrait<InfoType> where InfoType:PausableConditionalTraitInfo
    {

        protected PausableConditionalTrait(InfoType info) : base(info)
        {

        }

    }
}