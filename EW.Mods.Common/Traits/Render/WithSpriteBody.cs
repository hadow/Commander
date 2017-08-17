using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Default trait for rendering sprite-based actors.
    /// </summary>
    public class WithSpriteBodyInfo : ConditionalTraitInfo,IRenderActorPreviewSpritesInfo,Requires<RenderSpritesInfo>
    {

        /// <summary>
        /// Animation to play when the actor is created.
        /// </summary>
        [SequenceReference]
        public readonly string StartSequence = null;

        /// <summary>
        /// Animation to play when the actor is idle.
        /// </summary>
        [SequenceReference]
        public readonly string Sequence = "idle";

        /// <summary>
        /// Pause animation when actor is disabled.
        /// </summary>
        public readonly bool PauseAnimationWhenDisabled = false;

        /// <summary>
        /// Identifier used to assign modifying traits to this sprite body.
        /// </summary>
        public readonly string Name = "body";
        public override object Create(ActorInitializer init)
        {
            return new WithSpriteBody(init, this);
        }

        public virtual IEnumerable<IActorPreview> RenderPreviewSprites(ActorPreviewInitializer init,RenderSpritesInfo rs,string image,int facings,PaletteReference p)
        {

        }
    }

    public class WithSpriteBody:UpgradableTrait<WithSpriteBodyInfo>
    {
        public WithSpriteBody(ActorInitializer init,WithSpriteBodyInfo info) : this(init, info, () => 0) { }

        protected WithSpriteBody(ActorInitializer init,WithSpriteBodyInfo info,Func<int> baseFacing) : base(info) { }
    }
}