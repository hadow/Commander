using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.NetWork;
using System.Drawing;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common
{
    public static class ActorExts
    {
        public static bool IsAtGroundLevel(this Actor self)
        {
            if (self.IsDead)
                return false;

            if (self.OccupiesSpace == null)
                return false;

            if (!self.IsInWorld)
                return false;

            var map = self.World.Map;
            if (!map.Contains(self.Location))
                return false;

            return map.DistanceAboveTerrain(self.CenterPosition).Length == 0;
        }

        /// <summary>
        /// 友好双方
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toActor"></param>
        /// <returns></returns>
        public static bool AppearsFriendlyTo(this Actor self,Actor toActor)
        {
            var stance = toActor.Owner.Stances[self.Owner];
            if(stance == Stance.Ally)
            {
                return true;
            }

            if (self.EffectiveOwner != null && self.EffectiveOwner.Disguised && !toActor.Info.HasTraitInfo<IgnoresDisguiseInfo>())
                return toActor.Owner.Stances[self.EffectiveOwner.Owner] == Stance.Ally;

            return stance == Stance.Ally;
        }

        /// <summary>
        /// 敌对双方
        /// </summary>
        /// <param name="self"></param>
        /// <param name="toActor"></param>
        /// <returns></returns>
        public static bool AppearsHostileTo(this Actor self,Actor toActor)
        {
            var stance = toActor.Owner.Stances[self.Owner];
            if (stance == Stance.Ally)
                return false;

            if (self.EffectiveOwner != null && self.EffectiveOwner.Disguised && !toActor.Info.HasTraitInfo<IgnoresDisguiseInfo>())
                return toActor.Owner.Stances[self.EffectiveOwner.Owner] == Stance.Enemy;

            return stance == Stance.Enemy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="order"></param>
        /// <param name="targetLine"></param>
        /// <returns></returns>
        public static Target ResolveFrozenActorOrder(this Actor self,Order order,Color targetLine)
        {

            if (order.Target.Type != TargetT.FrozenActor)
                return order.Target;

            var frozen = order.Target.FrozenActor;

            self.SetTargetLine(frozen,targetLine,true);

            return Target.Invalid;
        }

        public static void NotifyBlocker(this Actor self,CPos position)
        {
            NotifyBlocker(self, self.World.ActorMap.GetActorsAt(position));
        }

        public static void NotifyBlocker(this Actor self,IEnumerable<Actor> blockers)
        {
            foreach(var blocker in blockers)
            {
                foreach (var moveBlocked in blocker.TraitsImplementing<INotifyBlockingMove>())
                    moveBlocked.OnNotifyBlockingMove(blocker, self);
            }
        }

        public static void NotifyBlocker(this Actor self,IEnumerable<CPos> positions)
        {
            NotifyBlocker(self, positions.SelectMany(p => self.World.ActorMap.GetActorsAt(p)));
        }

        public static CPos ClosestCell(this Actor self,IEnumerable<CPos> cells)
        {
            return cells.MinByOrDefault(c => (self.Location - c).LengthSquard);
        }
    }
}