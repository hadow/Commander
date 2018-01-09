using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class LobbyPrerequisiteCheckboxInfo:ITraitInfo
    {

        public object Create(ActorInitializer init) { return new LobbyPrerequisiteCheckbox(); }
    }

    public class LobbyPrerequisiteCheckbox
    {

    }
}