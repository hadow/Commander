using System;
using System.Linq;
using EW.Traits;
using EW.Activities;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Replaces the sprite during construction/deploy/undeploy.
    /// </summary>
    public class WithMakeAnimationInfo : ITraitInfo,Requires<WithSpriteBodyInfo>
    {
        [SequenceReference]
        public readonly string Sequence = "make";

        /// <summary>
        /// The condition to grant to self while the make animation is playing.
        /// </summary>
        [GrantedConditionReference]
        public readonly string Condition = null;


        /// <summary>
        /// Apply to sprite bodies with these names.
        /// </summary>
        public readonly string[] BodyNames = { "body" };

        public object Create(ActorInitializer init) { return new WithMakeAnimation(init,this); }
    }

    public class WithMakeAnimation:INotifyCreated,INotifyDeployTriggered
    {

        readonly WithMakeAnimationInfo info;
        readonly WithSpriteBody[] wsbs;

        ConditionManager conditionManager;
        int token = ConditionManager.InvalidConditionToken;

        public WithMakeAnimation(ActorInitializer init,WithMakeAnimationInfo info)
        {
            this.info = info;
            var self = init.Self;

            wsbs = self.TraitsImplementing<WithSpriteBody>().Where(w => info.BodyNames.Contains(w.Info.Name)).ToArray();

        }

        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();
            var building = self.TraitOrDefault<Building>();
            if (building != null && !building.SkipMakeAnimation)
                Forward(self, () => building.NotifyBuildingComplete(self));
        }

        public void Forward(Actor self,Action onComplete)
        {
            if (conditionManager != null && !string.IsNullOrEmpty(info.Condition) && token == ConditionManager.InvalidConditionToken)
                token = conditionManager.GrantCondition(self, info.Condition);

            var wsb = wsbs.FirstEnabledTraitOrDefault();

            if (wsb == null)
                return;


            wsb.PlayCustomAnimation(self, info.Sequence, () =>
            {
                if (token != ConditionManager.InvalidConditionToken)
                    token = conditionManager.RevokeCondition(self, token);

                onComplete();
            });
        }


        public void Reverse(Actor self,Action onComplete)
        {
            if (conditionManager != null && !string.IsNullOrEmpty(info.Condition) && token == ConditionManager.InvalidConditionToken)
                token = conditionManager.GrantCondition(self, info.Condition);

            var wsb = wsbs.FirstEnabledTraitOrDefault();
            if (wsb == null)
                return;

            wsb.PlayCustomAnimationBackwards(self, info.Sequence, () =>
            {
                if (token != ConditionManager.InvalidConditionToken)
                    token = conditionManager.RevokeCondition(self, token);

                onComplete();
            });
        }


        public void Reverse(Actor self,Activity activity,bool queued = true)
        {
            Reverse(self, () =>
            {

                var wsb = wsbs.FirstEnabledTraitOrDefault();

                if (wsb != null)
                    wsb.DefaultAnimation.PlayFetchIndex(info.Sequence, () => 0);

                if (conditionManager != null && !string.IsNullOrEmpty(info.Condition))
                    token = conditionManager.GrantCondition(self, info.Condition);

                self.QueueActivity(queued, activity);

            });
        }


        void INotifyDeployTriggered.Deploy(Actor self, bool skipMakeAnim)
        {
            var notified = false;

            var notify = self.TraitsImplementing<INotifyDeployComplete>();

            if (skipMakeAnim)
            {
                foreach (var n in notify)
                    n.FinishedDeploy(self);

                return;
            }

            foreach(var wsb in wsbs)
            {
                if (wsb.IsTraitDisabled)
                    continue;

                wsb.PlayCustomAnimation(self, info.Sequence, () =>
                {
                    if (notified)
                        return;
                    foreach(var n in notify)
                    {
                        n.FinishedDeploy(self);
                        notified = true;
                    }
                });
            }
        }



        void INotifyDeployTriggered.Undeploy(Actor self, bool skipMakeAnim)
        {
            var notified = false;

            var notify = self.TraitsImplementing<INotifyDeployComplete>();

            if (skipMakeAnim)
            {
                foreach (var n in notify)
                    n.FinishedUndeploy(self);

                return;
            }

            foreach(var wsb in wsbs)
            {
                if (wsb.IsTraitDisabled)
                    continue;

                wsb.PlayCustomAnimationBackwards(self, info.Sequence, () =>
                {
                    if (notified)
                        return;

                    foreach(var n in notify)
                    {
                        n.FinishedUndeploy(self);
                        notified = true;
                    }
                });
            }
        }
    }
}