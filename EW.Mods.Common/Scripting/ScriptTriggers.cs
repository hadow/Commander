using System;
using System.Collections.Generic;
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