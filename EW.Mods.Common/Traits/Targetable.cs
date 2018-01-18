using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class IgnoresCloakInfo:TraitInfo<IgnoresCloak>{}
    class IgnoresCloak{}


    /// <summary>
    /// Actor can be targeted.
    /// </summary>
    public class TargetableInfo : ConditionalTraitInfo,ITargetableInfo
    {
        /// <summary>
        /// Target type.Used for filtering (in) valid targets.
        /// </summary>
        public readonly HashSet<string> TargetTypes = new HashSet<string>();

        public HashSet<string> GetTargetTypes() { return TargetTypes; }

        public bool RequiresForceFire = false;

        public override object Create(ActorInitializer init)
        {
            return new Targetable(init.Self, this);

        }
    }
    public class Targetable:ConditionalTrait<TargetableInfo>,ITargetable
    {

        protected static readonly string[] None = new string[] { };

        protected Cloak[] cloaks;



        public Targetable(Actor self,TargetableInfo info) : base(info) { }


        protected override void Created(Actor self)
        {
            cloaks = self.TraitsImplementing<Cloak>().ToArray();
            base.Created(self);
        }

        public virtual bool TargetableBy(Actor self,Actor viewer){

            if (IsTraitDisabled)
                return false;

            if (!cloaks.Any() || (!viewer.IsDead && viewer.Info.HasTraitInfo<IgnoresCloakInfo>()))
                return true;

            return cloaks.All(c => c.IsTraitDisabled || c.IsVisible(self, viewer.Owner));

        }

        public virtual HashSet<string> TargetTypes{ get { return Info.TargetTypes; }}


        public bool RequiresForceFire{ get { return Info.RequiresForceFire; }}
    }
}