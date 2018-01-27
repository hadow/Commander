using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using EW.Activities;
namespace EW.Mods.Common.Traits.Render
{
    class WithHarvestOverlayInfo:ITraitInfo,Requires<RenderSpritesInfo>,Requires<BodyOrientationInfo>
    {
        [SequenceReference]
        public readonly string Sequence = "harvest";

        public readonly WVec LocalOffset = WVec.Zero;

        [PaletteReference]
        public readonly string Palette = "effect";

        public object Create(ActorInitializer init)
        {
            return new WithHarvestOverlay(init.Self,this);
        }
    }

    class WithHarvestOverlay:INotifyHarvesterAction
    {
        readonly WithHarvestOverlayInfo info;
        readonly Animation anim;
        bool visible;

        public WithHarvestOverlay(Actor self,WithHarvestOverlayInfo info)
        {
            this.info = info;
            var rs = self.Trait<RenderSprites>();
            var body = self.Trait<BodyOrientation>();

            anim = new Animation(self.World, rs.GetImage(self), RenderSprites.MakeFacingFunc(self));
            anim.IsDecoration = true;
            anim.Play(info.Sequence);
            rs.Add(new AnimationWithOffset(anim,
                () => body.LocalToWorld(info.LocalOffset.Rotate(body.QuantizeOrientation(self, self.Orientation))),
                () => !visible,
                p => ZOffsetFromCenter(self, p, 0)),
                info.Palette);
        }

        public void Harvested(Actor self,ResourceType resource)
        {
            if (visible)
                return;

            visible = true;
            anim.PlayThen(info.Sequence, () => visible = false);
        }

        public void Docked() { }

        public void Undocked() { }




        public static int ZOffsetFromCenter(Actor self,WPos pos,int offset)
        {
            var delta = self.CenterPosition - pos;
            return delta.Y + delta.Z + offset;
        }



    }
}