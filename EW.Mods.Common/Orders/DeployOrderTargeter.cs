using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Orders
{
    public class DeployOrderTargeter:IOrderTargeter
    {
        public string OrderID { get; private set; }

        public int OrderPriority { get; private set; }

        public bool IsQueued { get; protected set; }

        public DeployOrderTargeter(string order,int priority) : this(order, priority, () => "deploy") { }

        public DeployOrderTargeter(string order,int priority,Func<string> cursor)
        {
            OrderID = order;
            OrderPriority = priority;

        }
        public bool TargetOverridesSelection(TargetModifiers modifiers) { return true; }

        public bool CanTarget(Actor self,Target target,List<Actor> othersAtTarget,ref TargetModifiers modifiers,ref string cursor)
        {
            return false;
        }
    }
}