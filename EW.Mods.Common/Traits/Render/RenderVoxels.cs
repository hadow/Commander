using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Traits.Render
{

    public class RenderVoxelsInfo : ITraitInfo,IRenderActorPreviewInfo,Requires<BodyOrientationInfo>
    {

        /// <summary>
        /// Defaults to the actor name
        /// </summary>
        public readonly string Image = null;

        /// <summary>
        /// Custom palette name
        /// </summary>
        [PaletteReference]
        public readonly string Palette = null;

        /// <summary>
        /// Custom PlayerColorPalette:BaseName.
        /// </summary>
        public readonly string PlayerPalette = "player";
        public readonly string NormalsPalette = "normals";
        public readonly string ShadowPalette = "shadow";

        public readonly WAngle LightPitch = WAngle.FromDegrees(50);
        public readonly WAngle LightYaw = WAngle.FromDegrees(240);

        public readonly float[] LightAmbientColor = { 0.6f, 0.6f, 0.6f };
        public readonly float[] LightDiffuseColor = { 0.4f, 0.4f, 0.4f };

        /// <summary>
        /// Change the image size.
        /// </summary>
        public readonly float Scale = 12;

        
        public virtual object Create(ActorInitializer init) { return new RenderVoxels(init.Self, this); }

        public virtual IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init)
        {
            var body = init.Actor.TraitInfo<BodyOrientationInfo>();
            var faction = init.Get<FactionInit, string>();
            var ownerName = init.Get<OwnerInit>().PlayerName;
            var sequenceProvider = init.World.Map.Rules.Sequences;
            var image = Image ?? init.Actor.Name;
            var facings = body.QuantizedFacings == -1 ? init.Actor.TraitInfo<IQuantizeBodyOrientationInfo>().QuantizedBodyFacings(init.Actor, sequenceProvider, faction) :
                body.QuantizedFacings;

            var palette = init.WorldRenderer.Palette(Palette ?? PlayerPalette + ownerName);

            var components = init.Actor.TraitInfos<IRenderActorPreviewVoxelsInfo>().SelectMany(rvpi => rvpi.RenderPreviewVoxels(init, this, image, init.GetOrientation(), facings, palette)).ToArray();

            yield return new ModelPreview(components, WVec.Zero, 0, Scale, LightPitch, LightYaw, LightAmbientColor, LightDiffuseColor, body.CameraPitch, palette, init.WorldRenderer.Palette(NormalsPalette), init.WorldRenderer.Palette(ShadowPalette));
        }

    }


    public class RenderVoxels : IRender, INotifyOwnerChanged,ITick
    {

        class AnimationWrapper
        {
            readonly ModelAnimation model;
            bool cachedVisible;
            WVec cachedOffset;

            public AnimationWrapper(ModelAnimation model)
            {
                this.model = model;
            }

            public bool Tick()
            {

                //Return to the caller whether the renderable position or size has changed.
                var visible = model.IsVisible;
                var offset = model.OffsetFunc != null ? model.OffsetFunc() : WVec.Zero;

                var updated = visible != cachedVisible || offset != cachedOffset;
                cachedVisible = visible;
                cachedOffset = offset;
                return updated;
            }
        }

        readonly List<ModelAnimation> components = new List<ModelAnimation>();
        readonly Actor self;
        public readonly RenderVoxelsInfo Info;
        readonly BodyOrientation body;
        readonly WRot camera;
        readonly WRot lightSource;

        bool initializePalettes = true;

        protected PaletteReference colorPalette, normalsPalette, shadowPalette;

        readonly Dictionary<ModelAnimation, AnimationWrapper> wrappers = new Dictionary<ModelAnimation, AnimationWrapper>();
        public RenderVoxels(Actor self, RenderVoxelsInfo info)
        {
            this.self = self;
            Info = info;
            body = self.Trait<BodyOrientation>();

            camera = new WRot(WAngle.Zero, body.CameraPitch - new WAngle(256), new WAngle(256));
            lightSource = new WRot(WAngle.Zero, new WAngle(256) - info.LightPitch, info.LightYaw);
        }


        void ITick.Tick(Actor self)
        {
            var updated = false;
            foreach (var w in wrappers.Values)
                updated |= w.Tick();

            if (updated)
                self.World.ScreenMap.AddOrUpdate(self);
        }

        public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
        {
            if (initializePalettes)
            {
                var paletteName = Info.Palette ?? Info.PlayerPalette + self.Owner.InternalName;
                colorPalette = wr.Palette(paletteName);
                normalsPalette = wr.Palette(Info.NormalsPalette);
                shadowPalette = wr.Palette(Info.ShadowPalette);
                initializePalettes = false;
            }

            return new IRenderable[]
            {
                new ModelRenderable(components,self.CenterPosition,0
                                    ,camera,Info.Scale,lightSource,Info.LightAmbientColor
                                    ,Info.LightDiffuseColor,colorPalette,normalsPalette,shadowPalette)
            };
        }


        IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
        {
            var pos = self.CenterPosition;
            foreach(var c in components)
            {
                if (c.IsVisible)
                    yield return c.ScreenBounds(pos, wr, Info.Scale);
            }
        }

        public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            initializePalettes = true;
        }

        public string Image { get { return Info.Image ?? self.Info.Name; } }

        public void Add(ModelAnimation ma)
        {
            components.Add(ma);
            wrappers.Add(ma, new AnimationWrapper(ma));
        }
    }
    
}