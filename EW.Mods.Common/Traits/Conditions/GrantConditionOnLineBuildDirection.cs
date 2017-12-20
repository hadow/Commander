using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class GrantConditionOnLineBuildDirectionInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new GrantConditionOnLineBuildDirection();
        }
    }


    public class GrantConditionOnLineBuildDirection
    {

    }
}