﻿using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AirstrikePowerInfo : SupportPowerInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class AirstrikePower:SupportPower
    {
        public AirstrikePower(Actor self,AirstrikePowerInfo info) : base(self, info) { }
    }
}