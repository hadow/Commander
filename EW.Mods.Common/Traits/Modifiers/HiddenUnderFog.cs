using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class HiddenUnderFogInfo:HiddenUnderShroudInfo{

        public override object Create(ActorInitializer init)
        {
            return base.Create(init);
        }
    }

    public class HiddenUnderFog:HiddenUnderShroud
    {
        public HiddenUnderFog(HiddenUnderFogInfo info):base(info)
        {
            
        }
    }
}
