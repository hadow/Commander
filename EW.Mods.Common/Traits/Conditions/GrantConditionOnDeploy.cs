using System;
using System.Collections.Generic;
using System.Linq;
using EW.NetWork;
using EW.Traits;
using EW.Mods.Common.Activities;
using EW.Mods.Common.Orders;
namespace EW.Mods.Common.Traits
{
    public class GrantConditionOnDeployInfo:ITraitInfo
    {
        [GrantedConditionReference]
        [Desc("The condition to grant while the actor is undeployed.")]
        public readonly string UndeployedCondition = null;


        [FieldLoader.Require]
        [GrantedConditionReference]
        [Desc("The condition to grant after deploying and revoke before undeploying.")]
        public readonly string DeployedCondition = null;

        [Desc("The terrain types that this actor can deploy on. Leave empty to allow any.")]
        public readonly HashSet<string> AllowedTerrainTypes = new HashSet<string>();


        [Desc("Sound to play when deploying.")]
        public readonly string DeploySound = null;

        [Desc("Can this actor deploy on slopes?")]
        public readonly bool CanDeployOnRamps = false;

        [Desc("Sound to play when undeploying.")]
        public readonly string UndeploySound = null;

        [Desc("Can this actor undeploy?")]
        public readonly bool CanUndeploy = true;

        [Desc("Skip make/deploy animation?")]
        public readonly bool SkipMakeAnimation = false;

        [Desc("Cursor to display when able to (un)deploy the actor.")]
        public readonly string DeployCursor = "deploy";

        [Desc("Cursor to display when unable to (un)deploy the actor.")]
        public readonly string DeployBlockedCursor = "deploy-blocked";

        [Desc("Facing that the actor must face before deploying. Set to -1 to deploy regardless of facing.")]
        public readonly int Facing = -1;
        public object Create(ActorInitializer init)
        {
            return new GrantConditionOnDeploy(init,this);
        }

    }

    public enum DeployState { Undeployed, Deploying, Deployed, Undeploying }


    public class GrantConditionOnDeploy:IResolveOrder,IIssueOrder,INotifyCreated,IIssueDeployOrder
    {

        readonly Actor self;
        public readonly GrantConditionOnDeployInfo Info;
        readonly bool checkTerrainType;
        readonly bool canTurn;

        DeployState deployState;

        INotifyDeployTriggered[] notify;


        ConditionManager conditionManager;

        int deployedToken = ConditionManager.InvalidConditionToken;
        int undeployedToken = ConditionManager.InvalidConditionToken;

        public DeployState DeployState { get { return deployState; } }


        public GrantConditionOnDeploy(ActorInitializer init, GrantConditionOnDeployInfo info)
        {
            self = init.Self;
            Info = info;
            checkTerrainType = info.AllowedTerrainTypes.Count > 0;
            canTurn = self.Info.HasTraitInfo<IFacingInfo>();
            if (init.Contains<DeployStateInit>())
                deployState = init.Get<DeployStateInit, DeployState>();
        }

        void INotifyCreated.Created(Actor self){

            conditionManager = self.TraitOrDefault<ConditionManager>();

            notify = self.TraitsImplementing<INotifyDeployTriggered>().ToArray();

            switch (deployState)
            {
                case DeployState.Undeployed:
                    OnUndeployCompleted();
                    break;
                case DeployState.Deploying:
                    if (canTurn)
                        self.Trait<IFacing>().Facing = Info.Facing;

                    Deploy(true);
                    break;
                case DeployState.Deployed:
                    if (canTurn)
                        self.Trait<IFacing>().Facing = Info.Facing;

                    OnDeployCompleted();
                    break;
                case DeployState.Undeploying:
                    if (canTurn)
                        self.Trait<IFacing>().Facing = Info.Facing;

                    Undeploy(true);
                    break;
            }
        }

        /// <summary>Play deploy sound and animation.</summary>
        public void Deploy() { Deploy(false); }
        void Deploy(bool init)
        {
            // Something went wrong, most likely due to deploy order spam and the fact that this is a delayed action.
            if (!init && deployState != DeployState.Undeployed)
                return;

            if (!IsValidTerrain(self.Location))
                return;

            if (!string.IsNullOrEmpty(Info.DeploySound))
                WarGame.Sound.Play(SoundType.World, Info.DeploySound, self.CenterPosition);

            // Revoke condition that is applied while undeployed.
            if (!init)
                OnDeployStarted();

            // If there is no animation to play just grant the condition that is used while deployed.
            // Alternatively, play the deploy animation and then grant the condition.
            if (!notify.Any())
                OnDeployCompleted();
            else
                foreach (var n in notify)
                    n.Deploy(self, Info.SkipMakeAnimation);
        }


        /// <summary>Play undeploy sound and animation and after that revoke the condition.</summary>
        public void Undeploy() { Undeploy(false); }
        void Undeploy(bool init)
        {
            // Something went wrong, most likely due to deploy order spam and the fact that this is a delayed action.
            if (!init && deployState != DeployState.Deployed)
                return;

            if (!string.IsNullOrEmpty(Info.UndeploySound))
                WarGame.Sound.Play(SoundType.World, Info.UndeploySound, self.CenterPosition);

            if (!init)
                OnUndeployStarted();

            // If there is no animation to play just grant the condition that is used while undeployed.
            // Alternatively, play the undeploy animation and then grant the condition.
            if (!notify.Any())
                OnUndeployCompleted();
            else
                foreach (var n in notify)
                    n.Undeploy(self, Info.SkipMakeAnimation);
        }


        public bool IsValidTerrain(CPos location)
        {
            return IsValidTerrainType(location) && IsValidRampType(location);
        }

        bool IsValidTerrainType(CPos location)
        {
            if (!self.World.Map.Contains(location))
                return false;

            if (!checkTerrainType)
                return true;

            var terrainType = self.World.Map.GetTerrainInfo(location).Type;

            return Info.AllowedTerrainTypes.Contains(terrainType);
        }

        bool IsValidRampType(CPos location)
        {
            if (Info.CanDeployOnRamps)
                return true;

            var ramp = 0;
            if (self.World.Map.Contains(location))
            {
                var tile = self.World.Map.Tiles[location];
                var ti = self.World.Map.Rules.TileSet.GetTileInfo(tile);
                if (ti != null)
                    ramp = ti.RampType;
            }

            return ramp == 0;
        }

        void OnDeployStarted(){

            if (undeployedToken != ConditionManager.InvalidConditionToken)
                undeployedToken = conditionManager.RevokeCondition(self, undeployedToken);

            deployState = DeployState.Deploying;
        }

        void OnDeployCompleted(){

            if (conditionManager != null && !string.IsNullOrEmpty(Info.DeployedCondition) && deployedToken == ConditionManager.InvalidConditionToken)
                deployedToken = conditionManager.GrantCondition(self, Info.DeployedCondition);

            deployState = DeployState.Deployed;

        }


        void OnUndeployStarted(){

            if (deployedToken != ConditionManager.InvalidConditionToken)
                deployedToken = conditionManager.RevokeCondition(self, deployedToken);

            deployState = DeployState.Deploying;

        }

        void OnUndeployCompleted(){

            if (conditionManager != null && !string.IsNullOrEmpty(Info.UndeployedCondition) && undeployedToken == ConditionManager.InvalidConditionToken)
                undeployedToken = conditionManager.GrantCondition(self, Info.UndeployedCondition);

            deployState = DeployState.Undeployed;

        }


        void IResolveOrder.ResolveOrder(Actor self,Order order){
        
            if (order.OrderString != "GrantConditionOnDeploy" || deployState == DeployState.Deploying || deployState == DeployState.Undeploying)
                return;

            if (!order.Queued)
                self.CancelActivity();

            if (deployState == DeployState.Deployed && Info.CanUndeploy)
                self.QueueActivity(new UndeployForGrantedCondition(self, this));
            else if (deployState == DeployState.Undeployed)
                self.QueueActivity(new DeployForGrantedCondition(self, this));
        }


        public Order IssueOrder(Actor self, IOrderTargeter order, Target target, bool queued)
        {
            if (order.OrderID == "GrantConditionOnDeploy")
                return new Order(order.OrderID, self, queued);

            return null;
        }


        Order IIssueDeployOrder.IssueDeployOrder(Actor self)
        {
            return new Order("GrantConditionOnDeploy", self, false);
        }

        public IEnumerable<IOrderTargeter> Orders
        {
            get
            {
                yield return new DeployOrderTargeter("GrantConditionOnDeploy", 5,
              () => IsCursorBlocked() ? Info.DeployBlockedCursor : Info.DeployCursor);
            }
        }


        bool IsCursorBlocked()
        {
            return ((deployState == DeployState.Deployed) && !Info.CanUndeploy) || (!IsValidTerrain(self.Location) && (deployState != DeployState.Deployed));
        }

    }

    public class DeployStateInit : IActorInit<DeployState>
    {
        [FieldFromYamlKey]
        readonly DeployState value = DeployState.Deployed;
        public DeployStateInit() { }
        public DeployStateInit(DeployState init) { value = init; }
        public DeployState Value(World world) { return value; }
    }
}