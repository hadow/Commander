using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Can be used to make a unit partly uncontrollable by the player.
    /// </summary>
    public class RejectsOrdersInfo : ConditionalTraitInfo
    {

        public readonly HashSet<string> Reject = new HashSet<string>();

        public readonly HashSet<string> Except = new HashSet<string>();

        public override object Create(ActorInitializer init)
        {
            return new RejectsOrders(this);
        }
    }
    public class RejectsOrders : ConditionalTrait<RejectsOrdersInfo>
    {

        public RejectsOrders(RejectsOrdersInfo info) : base(info) { }

        public HashSet<string> Reject{ get { return Info.Reject; }}
        public HashSet<string> Excpet{ get { return Info.Except; }}


    }

    public static class RejectsOrdersExts{

        public static bool AcceptsOrder(this Actor self,string orderString){

            var r = self.TraitsImplementing<RejectsOrders>().Where(Exts.IsTraitEnabled).ToList();

            return !r.Any() || r.Any(t => t.Reject.Any() && !t.Reject.Contains(orderString)) || r.Any(t => t.Excpet.Contains(orderString));
        }
    }
}