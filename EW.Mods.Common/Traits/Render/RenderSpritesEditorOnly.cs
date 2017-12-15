using System;
using System.Collections.Generic;
using EW.Graphics;

namespace EW.Mods.Common.Traits.Render
{


    class RenderSpritesEditorOnlyInfo : RenderSpritesInfo
    {

    }

    class RenderSpritesEditorOnly:RenderSprites
    {


        public RenderSpritesEditorOnly(ActorInitializer init,RenderSpritesEditorOnlyInfo info) : base(init, info)
        {

        }

        public override IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
        {
            return base.Render(self, wr);
        }
    }
}