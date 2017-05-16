using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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