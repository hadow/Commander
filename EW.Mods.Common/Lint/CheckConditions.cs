using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;

namespace EW.Mods.Common.Lint
{
    public class CheckConditions:ILintRulesPass
    {

        public void Run(Action<string> emitError,Action<string> emitWarning,Ruleset rules)
        {
            foreach(var actorInfo in rules.Actors)
            {
                if (actorInfo.Key.StartsWith("^", StringComparison.Ordinal))
                    continue;

                var granted = new HashSet<string>();
                var consumed = new HashSet<string>();

                foreach(var trait in actorInfo.Value.TraitInfos<ITraitInfo>())
                {
                    var fieldConsumed = trait.GetType().GetFields().Where(x => x.HasAttribute<ConsumedConditionReferenceAttribute>());

                }
            }
        }

    }
}