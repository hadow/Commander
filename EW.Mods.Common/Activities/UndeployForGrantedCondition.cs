using System;
using EW.Mods.Common.Traits;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class UndeployForGrantedCondition:Activity
    {
        readonly GrantConditionOnDeploy deploy;

        public UndeployForGrantedCondition(Actor self, GrantConditionOnDeploy deploy)
        {
            this.deploy = deploy;
        }

        public override Activity Tick(Actor self)
        {
            IsInterruptible = false; // must DEPLOY from now.
            deploy.Undeploy();

            // Wait for deployment
            if (deploy.DeployState == DeployState.Undeploying)
                return this;

            return NextActivity;
        }
    }
}
