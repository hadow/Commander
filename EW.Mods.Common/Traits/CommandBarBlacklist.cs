using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class CommandBarBlacklistInfo : TraitInfo<CommandBarBlacklist>
    {

        public readonly bool DisableStop = true;

        public readonly bool DisableWaypointMode = true;
    }

    public class CommandBarBlacklist
    {
    }
}