using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// The actor stays invisible under the shroud.
    /// </summary>
    public class HiddenUnderShroudInfo:ITraitInfo,IDefaultVisibilityInfo
    {

        /// <summary>
        /// Players with these stances can always see the actor.
        /// </summary>
        public readonly Stance AlwaysVisibleStances = Stance.Ally;

        public VisibilityType Type = VisibilityType.Footprint;

        public virtual object Create(ActorInitializer init){
            return new HiddenUnderShroud(this);
        }

    }
    public class HiddenUnderShroud:IDefaultVisibility,IRenderModifier
    {
        protected readonly HiddenUnderShroudInfo Info;


        public HiddenUnderShroud(HiddenUnderShroudInfo info)
        {
            Info = info;
        }

        protected virtual bool IsVisibleInner(Actor self,Player byPlayer)
        {
            if (Info.Type == VisibilityType.Footprint)
                return byPlayer.Shroud.AnyExplored(self.OccupiesSpace.OccupiedCells());

            var pos = self.CenterPosition;
            if (Info.Type == VisibilityType.GroundPosition)
                pos -= new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos));

            return byPlayer.Shroud.IsExplored(pos);
        }

        
        public bool IsVisible(Actor self,Player byPlayer)
        {
            if (byPlayer == null)
                return true;

            var stance = self.Owner.Stances[byPlayer];
            return Info.AlwaysVisibleStances.HasStance(stance) || IsVisibleInner(self,byPlayer);
        }


        IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
        {
            return IsVisible(self, self.World.RenderPlayer) ? r : SpriteRenderable.None;
        }

        IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, WorldRenderer wr, IEnumerable<Rectangle> bounds)
        {
            return bounds;
        }
        
    }
}
