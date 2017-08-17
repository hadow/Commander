using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Traits.Render
{

    public class RenderVoxelsInfo : ITraitInfo
    {
        public readonly string Image = null;

        [PaletteReference]
        public readonly string Palette = null;

        public readonly string PlayerPalette = "player";

        public readonly string NormalsPalette = "normals";

        public readonly string ShadowPalette = "shadow";

        public readonly WAngle LightPitch = WAngle.FromDegrees(50);
        public readonly WAngle LightYaw = WAngle.FromDegrees(240);

        public readonly float[] LightAmbientColor = { 0.6f, 0.6f, 0.6f };
        public readonly float[] LightDiffuseColor = { 0.4f, 0.4f, 0.4f };

        public readonly float Scale = 12;
        public virtual object Create(ActorInitializer init) { return new RenderVoxels(init.Self, this); }
    }
    public class RenderVoxels : IRender, INotifyOwnerChanged
    {
        readonly List<ModelAnimation> components = new List<ModelAnimation>();
        readonly Actor self;
        readonly RenderVoxelsInfo info;
        readonly BodyOrientation body;
        readonly WRot camera;
        readonly WRot lightSource;

        bool initializePalettes = true;

        protected PaletteReference colorPalette, normalsPalette, shadowPalette;
        public RenderVoxels(Actor self, RenderVoxelsInfo info)
        {
            this.self = self;
            this.info = info;
            body = self.Trait<BodyOrientation>();

            camera = new WRot(WAngle.Zero, body.CameraPitch - new WAngle(256), new WAngle(256));
            lightSource = new WRot(WAngle.Zero, new WAngle(256) - info.LightPitch, info.LightYaw);
        }

        public IEnumerable<IRenderable> Render(Actor self, WorldRenderer wr)
        {
            if (initializePalettes)
            {
                var paletteName = info.Palette ?? info.PlayerPalette + self.Owner.InternalName;
                colorPalette = wr.Palette(paletteName);
                normalsPalette = wr.Palette(info.NormalsPalette);
                shadowPalette = wr.Palette(info.ShadowPalette);
                initializePalettes = false;
            }

            return new IRenderable[]
            {
                new ModelRenderable(components,self.CenterPosition,0,camera,info.Scale,lightSource,info.LightAmbientColor,info.LightDiffuseColor,colorPalette,normalsPalette,shadowPalette)
            };
        }

        public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            initializePalettes = true;
        }

        public string Image { get { return info.Image ?? self.Info.Name; } }

        public void Add(ModelAnimation ma) { components.Add(ma); }
    }
    
}