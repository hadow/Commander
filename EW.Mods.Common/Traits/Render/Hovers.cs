using System;
using System.Collections.Generic;
using EW.Graphics;
using System.Linq;
using System.Drawing;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class HoversInfo : ConditionalTraitInfo,Requires<IMoveInfo>
    {
        [Desc("Amount of Z axis changes in world units.")]
        public readonly int OffsetModifier = -43;

        public readonly int MinHoveringAltitude = 0;

        public override object Create(ActorInitializer init)
        {
            return new Hovers(this, init.Self);
        }
    }
    public class Hovers:ConditionalTrait<HoversInfo>,IRenderModifier
    {


        public Hovers(HoversInfo info,Actor self) : base(info) { }


        public IEnumerable<IRenderable> ModifyRender(Actor self,WorldRenderer wr,IEnumerable<IRenderable> r){

            if (self.World.Paused || IsTraitDisabled)
                return r;

            var visualOffset = self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length >= Info.MinHoveringAltitude
                                   ? (int)Math.Abs((self.ActorID + WarGame.LocalTick) / 5 % 4 - 1) - 1 : 0;

            var worldVisulaOffset = new WVec(0, 0, Info.OffsetModifier * visualOffset);

            return r.Select(a => a.OffsetBy(worldVisulaOffset));

        }


        IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self,WorldRenderer wr,IEnumerable<Rectangle> bounds){
            return bounds;
        }
    }
}