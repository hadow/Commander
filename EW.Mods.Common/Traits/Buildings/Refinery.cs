using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// 精练厂
    /// </summary>
    public class RefineryInfo : Requires<WithSpriteBodyInfo>,IAcceptResourcesInfo
    {
        public readonly int DockAngle = 0;

        public readonly CVec DockOffset = CVec.Zero;

        public readonly bool IsDragRequired = false;

        public readonly WVec DragOffset = WVec.Zero;

        public readonly int DragLength = 0;

        public readonly bool UseStorage = true;

        public readonly bool DiscardExcessResources = false;


        public readonly bool ShowTicks = true;
        public readonly int TickLifetime = 30;
        public readonly int TickVelocity = 2;
        public readonly int TickRate = 10;


        public virtual object Create(ActorInitializer init) { return new Refinery(init.Self,this); }
    }
    public class Refinery
    {

        public Refinery(Actor self,RefineryInfo info)
        {

        }
    }


}