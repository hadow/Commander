using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public enum Trigger
    {
        OnIdle,
        OnDamaged,
        OnKilled,
        OnProduction,
        OnOtherProduction,
        OnPlayerWon,
        OnPlayerLost,
    }

    public class ScriptTriggersInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ScriptTriggers(); }
    }

    class ScriptTriggers
    {
    }
}