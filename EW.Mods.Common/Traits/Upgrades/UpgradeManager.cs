using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{


    public class UpgradeManagerInfo : TraitInfo<UpgradeManager>
    {

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