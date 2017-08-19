using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using EW.Traits;
using EW.Graphics;
using EW.Primitives;
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

        static readonly Pair<DamageState, string>[] DamagePrefixes =
        {
            Pair.New(DamageState.Critical,"critical-"),
            Pair.New(DamageState.Heavy,"damaged-"),
            Pair.New(DamageState.Medium,"scratched-"),
            Pair.New(DamageState.Light,"scuffed-")

        };

        class AnimationWrapper
        {
            public readonly AnimationWithOffset Animation;
            public readonly string Palette;
            public readonly bool IsPlayerPalette;

            public PaletteReference PaletteReference { get; private set; }


            public AnimationWrapper(AnimationWithOffset animation,string palette,bool isPlayerPalette)
            {
                Animation = animation;
                Palette = palette;
                IsPlayerPalette = isPlayerPalette;
            }


            public bool IsVisible
            {
                get
                {
                    return Animation.DisableFunc == null || !Animation.DisableFunc();
                }
            }

            public void CachePalette(WorldRenderer wr,Player owner)
            {
                PaletteReference = wr.Palette(IsPlayerPalette ? Palette + owner.InternalName : Palette);
            }

            public void OwnerChanged()
            {
                //Update the palette reference next time we draw
                if (IsPlayerPalette)
                    PaletteReference = null;
            }

        }

        string cachedImage;
        readonly RenderSpritesInfo info;
        readonly string faction;
        readonly List<AnimationWrapper> anims = new List<AnimationWrapper>();
        public RenderSprites(ActorInitializer init, RenderSpritesInfo info)
        {
            this.info = info;
            faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : init.Self.Owner.Faction.InternalName;
        }
        public virtual void Tick(Actor self){}


        public string GetImage(Actor self)
        {
            if (cachedImage != null)
                return cachedImage;

            return cachedImage = info.GetImage(self.Info, self.World.Map.Rules.Sequences, faction);
        }

        public virtual IEnumerable<IRenderable> Render(Actor self,WorldRenderer wr)
        {
            foreach(var a in anims)
            {
                if (!a.IsVisible)
                    continue;

                if(a.PaletteReference == null)
                {
                    var owner = self.EffectiveOwner != null && self.EffectiveOwner.Disguised ? self.EffectiveOwner.Owner : self.Owner;
                    a.CachePalette(wr, owner);

                }

                foreach (var r in a.Animation.Render(self, wr, a.PaletteReference, info.Scale))
                    yield return r;
            }
        }

        public virtual void OnOwnerChanged(Actor self,Player oldOwner,Player newOwner)
        {
            UpdatePalette();
        }

        public void UpdatePalette()
        {
            foreach (var anim in anims)
                anim.OwnerChanged();
        }

        void IActorPreviewInitModifier.ModifyActorPreviewInit(Actor self, TypeDictionary inits)
        {
            if (!inits.Contains<FactionInit>())
                inits.Add(new FactionInit(faction));
        }


        public static string NormalizeSequence(Animation anim,DamageState state,string sequence)
        {
            sequence = UnnormalizeSequence(sequence);

            foreach(var s in DamagePrefixes)
            {
                if (state >= s.First && anim.HasSequence(s.Second + sequence))
                    return s.Second + sequence;
            }
            return sequence;
        }

        public static string UnnormalizeSequence(string sequence)
        {
            foreach(var s in DamagePrefixes)
            {
                if (sequence.StartsWith(s.Second, StringComparison.Ordinal))
                {
                    sequence = sequence.Substring(s.Second.Length);
                    break;
                }
            }
            return sequence;
        }
    }
}