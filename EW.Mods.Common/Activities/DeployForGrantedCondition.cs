using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class DeployForGrantedCondition:Activity
    {
        readonly GrantConditionOnDeploy deploy;
        readonly bool canTurn;

        public DeployForGrantedCondition(Actor self, GrantConditionOnDeploy deploy)
        {
            this.deploy = deploy;
            canTurn = self.Info.HasTraitInfo<IFacingInfo>();
        }


        protected override void OnFirstRun(Actor self)
        {
            // Turn to the required facing.
            if (deploy.Info.Facing != -1 && canTurn)
                QueueChild(new Turn(self, deploy.Info.Facing));
        }

        public override Activity Tick(Actor self)
        {
            // Do turn first, if needed.
            if (ChildActivity != null)
            {
                ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);
                return this;
            }

            // Without this, turn for facing deploy angle will be canceled and immediately deploy!
            if (IsCanceled)
                return NextActivity;

            if (IsInterruptible)
            {
                IsInterruptible = false; // must DEPLOY from now.
                deploy.Deploy();
                return this;
            }

            // Wait for deployment
            if (deploy.DeployState == DeployState.Deploying)
                return this;

            // Failed or success, we are going to NextActivity.
            // Deploy() at the first run would have put DeployState == Deploying so
            // if we are back to DeployState.Undeployed, it means deploy failure.
            // Parent activity will see the status and will take appropriate action.
            return NextActivity;
        }

    }
}
