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
        public HiddenUnderFog(HiddenUnderFogInfo info):base(info){}

        protected override bool IsVisibleInner(Actor self, Player byPlayer)
        {

            if (!byPlayer.Shroud.FogEnabled)
                return base.IsVisibleInner(self, byPlayer);

            if (Info.Type == VisibilityType.Footprint)
                return byPlayer.Shroud.AnyVisible(self.OccupiesSpace.OccupiedCells());

            var pos = self.CenterPosition;
            if (Info.Type == VisibilityType.GroundPosition)
                pos -= new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos));

            return byPlayer.Shroud.IsVisible(pos);


        }
    }
}
