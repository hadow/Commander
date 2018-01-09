using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits.Render
{

    public class WithVoxelBodyInfo:ConditionalTraitInfo,Requires<RenderVoxelsInfo>
    {
        public readonly string Sequence = "idle";

        public readonly bool ShowShadow = true;


        public override object Create(ActorInitializer init)
        {
            return new WithVoxelBody(init.Self, this);
        }
    }

    public class WithVoxelBody:ConditionalTrait<WithVoxelBodyInfo>
    {
        readonly ModelAnimation modelAnimation;
        readonly RenderVoxels rv;

        public WithVoxelBody(Actor self,WithVoxelBodyInfo info):base(info)
        {
            var body = self.Trait<BodyOrientation>();
            rv = self.Trait<RenderVoxels>();

            var model = self.World.ModelCache.GetModelSequence(rv.Image, info.Sequence);
            modelAnimation = new ModelAnimation(model, () => WVec.Zero,
                () => new[] { body.QuantizeOrientation(self, self.Orientation) },
                () => IsTraitDisabled, () => 0, info.ShowShadow);

            rv.Add(modelAnimation);
        }
    }
}