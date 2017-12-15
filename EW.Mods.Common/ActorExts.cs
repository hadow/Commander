using System;
using System.Collections.Generic;
using EW.Traits;
using EW.OpenGLES;
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
            return Target.Invalid;
        }
    }
}