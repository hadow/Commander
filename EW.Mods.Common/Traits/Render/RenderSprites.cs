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

    public class RenderSpritesInfo:ITraitInfo,IRenderActorPreviewInfo{

        /// <summary>
        /// The sequence name that defines the actor sprites,Defaults to the actor name.
        /// </summary>
        public readonly string Image = null;

        public readonly Dictionary<string, string> FactionImages = null;

        [PaletteReference]
        public readonly string Palette = null;

        [PaletteReference(true)]
        public readonly string PlayerPalette = "player";

        /// <summary>
        /// Change the sprite image size.
        /// </summary>
        public readonly float Scale = 1f;

        public virtual object Create(ActorInitializer init){
            return new RenderSprites(init, this);
        }

        public IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init)
        {
            var sequenceProvider = init.World.Map.Rules.Sequences;
            var faction = init.Get<FactionInit, string>();
            var ownerName = init.Get<OwnerInit>().PlayerName;
            var image = GetImage(init.Actor, sequenceProvider, faction);
            var palette = init.WorldRenderer.Palette(Palette ?? PlayerPalette + ownerName);

            var facings = 0;
            var body = init.Actor.TraitInfoOrDefault<BodyOrientationInfo>();

            if(body != null)
            {
                facings = body.QuantizedFacings;

                if(facings == -1)
                {
                    var qbo = init.Actor.TraitInfoOrDefault<IQuantizeBodyOrientationInfo>();
                    facings = qbo != null ? qbo.QuantizedBodyFacings(init.Actor, sequenceProvider, faction) : 1;
                }
            }

            foreach (var spi in init.Actor.TraitInfos<IRenderActorPreviewSpritesInfo>())
                foreach (var preview in spi.RenderPreviewSprites(init, this, image, facings, palette))
                    yield return preview;
        }

        public string GetImage(ActorInfo actor,SequenceProvider sequenceProvider,string faction)
        {
            if(FactionImages != null && !string.IsNullOrEmpty(faction))
            {
                string factionImage = null;

                if (FactionImages.TryGetValue(faction, out factionImage) && sequenceProvider.HasSequence(factionImage))
                    return factionImage;
            }
            return (Image ?? actor.Name).ToLowerInvariant();
                
        }
        
    }



    public class RenderSprites:ITick,IRender,INotifyOwnerChanged,IActorPreviewInitModifier
    {

        class AnimationWrapper
        {
            public readonly string Palette;
            public readonly bool IsPlayerPalette;

            public PaletteReference PaletteReference { get; private set; }


        }
        public RenderSprites(ActorInitializer init, RenderSpritesInfo info){}
        public virtual void Tick(Actor self){}

    }
}