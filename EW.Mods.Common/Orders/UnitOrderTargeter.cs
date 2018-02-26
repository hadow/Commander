using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Orders
{
    public abstract class UnitOrderTargeter:IOrderTargeter
    {
        readonly string cursor;

        readonly bool targetEnemyUnits, targetAllyUnits;

        public bool? ForceAttack = null;

        public string OrderID { get; private set; }

        public int OrderPriority { get; private set; }

        public UnitOrderTargeter(string order,int priority,string cursor,bool targetEnemyUnits,bool targetAllyUnits)
        {
            OrderID = order;
            OrderPriority = priority;
            this.cursor = cursor;
            this.targetEnemyUnits = targetEnemyUnits;
            this.targetAllyUnits = targetAllyUnits;

        }

        public abstract bool CanTargetActor(Actor self, Actor target, TargetModifiers modifiers, ref string cursor);

        public abstract bool CanTargetFrozenActor(Actor self, FrozenActor target, TargetModifiers modifiers, ref string cursor);
        public bool TargetOverridesSelection(TargetModifiers modifiers) { return true; }

        public bool CanTarget(Actor self,Target target,List<Actor> othersAtTarget,ref TargetModifiers modifiers,ref string cursor)
        {
            var type = target.Type;

            if (type != TargetT.Actor && type != TargetT.FrozenActor)
                return false;

            cursor = this.cursor;
            IsQueued = modifiers.HasModifier(TargetModifiers.ForceQueue);

            if (ForceAttack != null && modifiers.HasModifier(TargetModifiers.ForceAttack) != ForceAttack)
                return false;

            var owner = type == TargetT.FrozenActor ? target.FrozenActor.Owner : target.Actor.Owner;

            var playerRelationShip = self.Owner.Stances[owner];

            if (!modifiers.HasModifier(TargetModifiers.ForceAttack) && playerRelationShip == Stance.Ally && !targetAllyUnits)
                return false;

            if (!modifiers.HasModifier(TargetModifiers.ForceAttack) && playerRelationShip == Stance.Enemy && !targetEnemyUnits)
                return false;

            return type == TargetT.FrozenActor ? CanTargetFrozenActor(self, target.FrozenActor, modifiers, ref cursor) :
                CanTargetActor(self, target.Actor, modifiers, ref cursor);
        }

        public virtual bool IsQueued { get; protected set; }

    }
}