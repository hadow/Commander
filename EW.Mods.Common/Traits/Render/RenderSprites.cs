using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public interface IRenderActorPreviewSpritesInfo:ITraitInfo{
        IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init, RenderSpritesInfo rs, string image, int facings,PaletteReference p);
    }

    public class RenderSpritesInfo:ITraitInfo{

        /// <summary>
        /// The sequence name that defines the actor sprites,Defaults to the actor name.
        /// </summary>
        public readonly string Image = null;

        public readonly Dictionary<string, string> FactionImages = null;

        public virtual object Create(ActorInitializer init){
            return new RenderSprites(init, this);
        }
        
    }



    public class RenderSprites:ITick
    {
        public RenderSprites(ActorInitializer init, RenderSpritesInfo info){}
        public virtual void Tick(Actor self){}

    }
}