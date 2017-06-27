using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Scripting
{

    public class ScriptTriggersInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ScriptTriggers();
        }
    }
    class ScriptTriggers
    {
    }
}