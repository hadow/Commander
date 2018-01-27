using System;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Traits.Render;
namespace EW.Mods.Cnc.Traits.Render
{
    public class WithVoxelUnloadBodyInfo:ITraitInfo,Requires<RenderVoxelsInfo>
    {
        public readonly string UnloadSequence = "unload";

        public readonly string IdleSequence = "idle";

        public readonly bool ShowShadow = true;
        public object Create(ActorInitializer init)
        {
            return new WithVoxelUnloadBody(init.Self,this);
        }
    }

    public class WithVoxelUnloadBody:IAutoMouseBounds
    {
        public bool Docked;

        readonly ModelAnimation modelAnimation;
        readonly RenderVoxels rv;

        public WithVoxelUnloadBody(Actor self, WithVoxelUnloadBodyInfo info)
        {
            var body = self.Trait<BodyOrientation>();
            rv = self.Trait<RenderVoxels>();

            var idleModel = self.World.ModelCache.GetModelSequence(rv.Image, info.IdleSequence);
            modelAnimation = new ModelAnimation(idleModel, () => WVec.Zero, 
                () => new[] { body.QuantizeOrientation(self, self.Orientation) }, () => Docked, () => 0, info.ShowShadow);
            rv.Add(modelAnimation);

            var unloadModel = self.World.ModelCache.GetModelSequence(rv.Image, info.UnloadSequence);
            rv.Add(new ModelAnimation(unloadModel, () => WVec.Zero,
                () => new[] { body.QuantizeOrientation(self, self.Orientation) },
                () => !Docked, () => 0, info.ShowShadow));
                
        }

        Rectangle IAutoMouseBounds.AutoMouseoverBounds(Actor self, WorldRenderer wr)
        {
            return modelAnimation.ScreenBounds(self.CenterPosition, wr, rv.Info.Scale);
        }
    }
}