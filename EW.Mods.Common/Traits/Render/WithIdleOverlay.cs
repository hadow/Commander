using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Graphics;
using EW.Graphics;
namespace EW.Mods.Common.Traits.Render
{
    public class WithIdleOverlayInfo : UpgradableTraitInfo,IRenderActorPreviewSpritesInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init,RenderSpritesInfo rs,string image,int facings,PaletteReference p)
        {
            throw new NotImplementedException();
        }
    }
    public class WithIdleOverlay
    {
    }
}