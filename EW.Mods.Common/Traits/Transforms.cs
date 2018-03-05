using System;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Mods.Common.Orders;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{

    public class TransformsInfo : PausableConditionalTraitInfo
    {

        [Desc("Actor to transform into."), ActorReference, FieldLoader.Require]
        public readonly string IntoActor = null;

        [Desc("Facing that the actor must face before transforming.")]
        public readonly int Facing = 96;

        [Desc("Offset to spawn the transformed actor relative to the current cell.")]
        public readonly CVec Offset = CVec.Zero;

        [Desc("Cursor to display when able to (un)deploy the actor.")]
        public readonly string DeployCursor = "deploy";

        [Desc("Cursor to display when unable to (un)deploy the actor.")]
        public readonly string DeployBlockedCursor = "deploy-blocked";

        [VoiceReference] public readonly string Voice = "Action";

        [Desc("Sounds to play when the transformation is blocked.")]
        public readonly string[] NoTransformSounds = { };

        [Desc("Sounds to play when transforming.")]
        public readonly string[] TransformSounds = { };

        [Desc("Notification to play when transforming.")]
        public readonly string TransformNotification = null;

        [Desc("Notification to play when the transformation is blocked.")]
        public readonly string NoTransformNotification = null;


        public override object Create(ActorInitializer init)
        {
            return new Transforms(init, this);
        }
    }

    public class Transforms:PausableConditionalTrait<TransformsInfo>,IIssueOrder,IResolveOrder,IOrderVoice,IIssueDeployOrder
    {


        readonly Actor self;
        readonly BuildingInfo buildingInfo;
        readonly string faction;

        public Transforms(ActorInitializer init, TransformsInfo info)
            : base(info)
        {
            self = init.Self;
            buildingInfo = self.World.Map.Rules.Actors[info.IntoActor].TraitInfoOrDefault<BuildingInfo>();
            faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : self.Owner.Faction.InternalName;
        }

        public string VoicePhraseForOrder(Actor self, Order order)
        {
            return (order.OrderString == "DeployTransform") ? Info.Voice : null;
        }


        public bool CanDeploy()
        {
            if (IsTraitPaused || IsTraitDisabled)
                return false;

            var building = self.TraitOrDefault<Building>();
            if (building != null && building.Locked)
                return false;

            return buildingInfo == null || self.World.CanPlaceBuilding(Info.IntoActor, buildingInfo, self.Location + Info.Offset, self);
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                if (!IsTraitDisabled)
                    yield return new DeployOrderTargeter("DeployTransform", 5,
                        () => CanDeploy() ? Info.DeployCursor : Info.DeployBlockedCursor);
            }
        }


        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID == "DeployTransform")
                return new Order(order.OrderID, self, queued);

            return null;
        }


        Order IIssueDeployOrder.IssueDeployOrder(Actor self)
        {
            return new Order("DeployTransform", self, false);
        }


        void IResolveOrder.ResolveOrder(Actor self,Order order){

            if (order.OrderString == "DeployTransform" && !IsTraitPaused && !IsTraitDisabled)
                DeployTransform(order.Queued);
        }


        public void DeployTransform(bool queued)
        {
            if (!queued && !CanDeploy())
            {
                // Only play the "Cannot deploy here" audio
                // for non-queued orders
                foreach (var s in Info.NoTransformSounds)
                    WarGame.Sound.PlayToPlayer(SoundType.World, self.Owner, s);

                WarGame.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", Info.NoTransformNotification, self.Owner.Faction.InternalName);

                return;
            }

            if (!queued)
                self.CancelActivity();

            self.QueueActivity(new Transform(self, Info.IntoActor)
            {
                Offset = Info.Offset,
                Facing = Info.Facing,
                Sounds = Info.TransformSounds,
                Notification = Info.TransformNotification,
                Faction = faction
            });
        }

    }
}