using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AttackFrontalInfo : AttackBaseInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }

    public class AttackFrontal:AttackBase
    {

        public AttackFrontal(Actor self, AttackFrontalInfo info):base(self,info)
        {

        }
    }
}