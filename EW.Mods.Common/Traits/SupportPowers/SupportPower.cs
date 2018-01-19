using System;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{

    public abstract class SupportPowerInfo : PausableConditionalTraitInfo
    {
        public readonly int ChargeInterval = 0;
        public readonly string Icon = null;
        public readonly string Description = "";
        public readonly string LongDesc = "";
        public readonly bool AllowMultiple = false;
        public readonly bool OneShot = false;




    }
    public class SupportPower:PausableConditionalTrait<SupportPowerInfo>
    {
        public readonly Actor Self;
        readonly SupportPowerInfo info;
        protected RadarPing ping;

        public SupportPower(Actor self,SupportPowerInfo info) : base(info)
        {
            Self = self;
            this.info = info;
        }


        public virtual void Activate(Actor self,Order order,SupportPowerManager manager)
        {

        }
    }
}