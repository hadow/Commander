using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using System.Drawing;
using EW.OpenGLES;
using EW.Graphics;
using EW.Mods.Common.Graphics;
namespace EW.Mods.Common.Traits
{
    public enum RangeCircleMode{
        Maximum,
        Minimum,
    }

    /// <summary>
    /// Draw a circle indicating my weapon's range
    /// </summary>
    class RenderRangeCircleInfo:ITraitInfo,Requires<AttackBaseInfo>,IRulesetLoaded
    {
        public readonly string RangeCircleType = null;

        public readonly WDist FallbackRange = WDist.Zero;
        /// <summary>
        /// Which circle to show. Valid value  are 'Maximum',and 'Minimum'
        /// </summary>
        public readonly RangeCircleMode RangeCircleMode = RangeCircleMode.Maximum;

        public readonly Color Color = Color.FromArgb(128, Color.Yellow);

        public readonly Color BorderColor = Color.FromArgb(96, Color.Black);
        
        /// <summary>
        /// Computed range.
        /// </summary>
        Lazy<WDist> range;


        public object Create(ActorInitializer init)
        {
            return new RenderRangeCircle(init.Self,this);
        }


        public void RulesetLoaded(Ruleset rules,ActorInfo ai){

            range = Exts.Lazy(() =>
            {
                var armaments = ai.TraitInfos<ArmamentInfo>().Where(a => a.EnabledByDefault);

                if (!armaments.Any())
                    return FallbackRange;

                return armaments.Max(a => a.ModifiedRange);
            });

        }
    }



    class RenderRangeCircle:IRenderAboveShroudWhenSelected
    {

        public readonly RenderRangeCircleInfo Info;

        readonly Actor self;

        readonly AttackBase attack;

        public RenderRangeCircle(Actor self, RenderRangeCircleInfo info){

            Info = info;

            this.self = self;

            attack = self.Trait<AttackBase>();
        }


        public IEnumerable<IRenderable> RangeCircleRenderables(WorldRenderer wr){

            if (!self.Owner.IsAlliedWith(self.World.RenderPlayer))
                yield break;

            var rang = Info.RangeCircleMode == RangeCircleMode.Maximum ? attack.GetMaximumRange() : attack.GetMinimumRange();

            if (rang == WDist.Zero)
                yield break;

            yield return new RangeCircleRenderable(self.CenterPosition, rang, 0, Info.Color, Info.BorderColor);

        }

        IEnumerable<IRenderable> IRenderAboveShroudWhenSelected.RenderAboveShroud(Actor self,WorldRenderer wr){

            return RangeCircleRenderables(wr);
        }
    }
}