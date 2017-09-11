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
        public readonly WVec[] TargetableOffsets = { WVec.Zero };

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
            throw new NotImplementedException();
        }
    }
    public class HitShape:ConditionalTrait<HitShapeInfo>
    {
        BodyOrientation orientation;
        ITargetableCells targetableCells;
        public HitShape(Actor actor,HitShapeInfo info) : base(info)
        {

        }

        protected override void Created(Actor self)
        {
            orientation = self.Trait<BodyOrientation>();
            targetableCells = self.TraitOrDefault<ITargetableCells>();
            base.Created(self);
        }
    }
}