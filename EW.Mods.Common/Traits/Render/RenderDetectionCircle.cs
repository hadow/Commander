using System;
using System.Collections.Generic;
using EW.Traits;
using System.Drawing;
using System.Linq;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Graphics;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    class RenderDetectionCircleInfo : ITraitInfo,Requires<DetectCloakedInfo>
    {
        [Desc("WAngle the Radar update line advances per tick.")]
        public readonly WAngle UpdateLineTick = new WAngle(-1);

        public readonly int TrailCount = 0;

        public readonly Color Color = Color.FromArgb(128, Color.LimeGreen);

        public readonly Color ContrastColor = Color.FromArgb(96, Color.Black);


        public object Create(ActorInitializer init)
        {
            return new RenderDetectionCircle(init.Self, this);
        }
    }
    class RenderDetectionCircle:ITick,IRenderAboveShroudWhenSelected
    {

        readonly RenderDetectionCircleInfo info;
        WAngle lineAngle;
        
        public RenderDetectionCircle(Actor self,RenderDetectionCircleInfo info)
        {
            this.info = info;
        }

        void ITick.Tick(Actor self)
        {
            lineAngle += info.UpdateLineTick;
        }

        IEnumerable<IRenderable> IRenderAboveShroudWhenSelected.RenderAboveShroud(Actor self, WorldRenderer wr)
        {
            if (!self.Owner.IsAlliedWith(self.World.RenderPlayer))
                yield break;

            var range = self.TraitsImplementing<DetectCloaked>()
                .Where(a => !a.IsTraitDisabled)
                .Select(a => a.Info.Range)
                .Append(WDist.Zero).Max();

            if (range == WDist.Zero)
                yield break;

            yield return new DetectionCircleRenderable(self.CenterPosition,
                range,
                0,
                info.TrailCount,
                info.UpdateLineTick,
                lineAngle,
                info.Color,
                info.ContrastColor);

        }


        bool IRenderAboveShroudWhenSelected.SpatiallyPartitionable { get { return false; } }



    }
}