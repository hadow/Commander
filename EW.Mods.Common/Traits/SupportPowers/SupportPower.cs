using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class SupportPowerInfo : UpgradableTraitInfo
    {

    }
    public class SupportPower:UpgradableTrait<SupportPowerInfo>
    {
        public SupportPower(Actor self,SupportPowerInfo info) : base(info)
        {

        }
    }
}