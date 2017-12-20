using System;


namespace EW.Mods.Common.Traits
{
    public class LevelUpCrateActionInfo : CrateActionInfo
    {

    }

    public class LevelUpCrateAction:CrateAction
    {

        public LevelUpCrateAction(Actor self,LevelUpCrateActionInfo info) : base(self, info) { }
    }
}