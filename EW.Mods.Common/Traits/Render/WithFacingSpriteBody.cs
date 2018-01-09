using EW.Mods.Common.Graphics;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WithFacingSpriteBodyInfo : WithSpriteBodyInfo, Requires<BodyOrientationInfo>,Requires<IFacingInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithFacingSpriteBody(init,this);
        }
    }

    public class WithFacingSpriteBody:WithSpriteBody
    {
        public WithFacingSpriteBody(ActorInitializer init,WithFacingSpriteBodyInfo info) : base(init, info, RenderSprites.MakeFacingFunc(init.Self)) { }

    }
}