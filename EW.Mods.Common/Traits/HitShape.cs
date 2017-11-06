using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.HitShapes;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Shape of actor for targeting and damage calculation.
    /// </summary>
    public class HitShapeInfo : ConditionalTraitInfo,Requires<BodyOrientationInfo>
    {
        /// <summary>
        /// Create a targetable position for each offset listed here(relative to CenterPosition)
        /// </summary>
        public readonly WVec[] TargetableOffsets = { WVec.Zero };


        /// <summary>
        /// Create a targetable position at the center of each occupied cell,Stacks with TargetableOffsets
        /// </summary>
        public readonly bool UseTargetableCellsOffsets = false;

        [FieldLoader.LoadUsing("LoadShape")]
        public readonly IHitShape Type;

        static object LoadShape(MiniYaml yaml)
        {
            IHitShape ret;

            var shapeNode = yaml.Nodes.FirstOrDefault(n => n.Key == "Type");
            var shape = shapeNode != null ? shapeNode.Value.Value : string.Empty;

            if (!string.IsNullOrEmpty(shape))
            {
                try
                {
                    ret = WarGame.CreateObject<IHitShape>(shape + "Shape");
                    FieldLoader.Load(ret, shapeNode.Value);
                }
                catch(YamlException e)
                {
                    throw new YamlException("HitShape {0}:{1}".F(shape, e.Message));
                }

            }
            else
                ret = new CircleShape();

            ret.Initialize();

            return ret;
        }
        public override object Create(ActorInitializer init)
        {
            return new HitShape(init.Self, this);
        }
    }
    public class HitShape:ConditionalTrait<HitShapeInfo>,ITargetablePositions
    {
        BodyOrientation orientation;
        ITargetableCells targetableCells;

        public HitShape(Actor actor,HitShapeInfo info) : base(info)
        {
            orientation = actor.Trait<BodyOrientation>();
            targetableCells = actor.TraitOrDefault<ITargetableCells>();
        }

        protected override void Created(Actor self)
        {
            orientation = self.Trait<BodyOrientation>();
            targetableCells = self.TraitOrDefault<ITargetableCells>();
            base.Created(self);
        }


        IEnumerable<WPos> ITargetablePositions.TargetablePositions(Actor self){


            if (IsTraitDisabled)
                yield break;
            if (Info.UseTargetableCellsOffsets && targetableCells !=null  ){
                foreach (var c in targetableCells.TargetableCells())
                    yield return self.World.Map.CenterOfCell(c.First);
            }

            foreach (var o in Info.TargetableOffsets){
                var offset = orientation.LocalToWorld(o.Rotate(orientation.QuantizeOrientation(self, self.Orientation)));
                yield return self.CenterPosition + offset;
            }
        }
    }
}