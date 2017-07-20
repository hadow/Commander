using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Attach this to a unit to enable dynamic upgrades by warheads,experience,crates,support power,etc.
    /// </summary>
    public class UpgradeManagerInfo : TraitInfo<UpgradeManager>,IRulesetLoaded
    {
        public void RulesetLoaded(Ruleset rules,ActorInfo info)
        {

        }

    }
    public class UpgradeManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="upgrade"></param>
        /// <param name="source"></param>
        public void RevokeUpgrade(Actor self,string upgrade,object source)
        {

        }

        public void GrantUpgrade(Actor self,string upgrade,object source)
        {

        }
    }
}