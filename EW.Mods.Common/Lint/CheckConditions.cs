using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Traits;
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
                    var fieldConsumed = trait.GetType().GetFields()
                        .Where(x => x.HasAttribute<ConsumedConditionReferenceAttribute>())
                        .SelectMany(f=>LintExts.GetFieldValues(trait,f,emitError));

                    var propertyConsumed = trait.GetType().GetProperties()
                        .Where(x => x.HasAttribute<ConsumedConditionReferenceAttribute>())
                        .SelectMany(p => LintExts.GetPropertyValues(trait, p, emitError));

                    var fieldGranted = trait.GetType().GetFields()
                        .Where(x => x.HasAttribute<GrantedConditionReferenceAttribute>())
                        .SelectMany(f => LintExts.GetFieldValues(trait, f, emitError));

                    var propertyGranted = trait.GetType().GetProperties()
                        .Where(x => x.HasAttribute<GrantedConditionReferenceAttribute>())
                        .SelectMany(f => LintExts.GetPropertyValues(trait, f, emitError));

                    foreach(var c in fieldConsumed.Concat(propertyConsumed))
                    {
                        if (!string.IsNullOrEmpty(c))
                            consumed.Add(c);
                    }

                    foreach(var g in propertyGranted.Concat(fieldGranted))
                    {
                        if (!string.IsNullOrEmpty(g))
                            granted.Add(g);
                    }



                }

                var unconsumed = granted.Except(consumed);
                if (unconsumed.Any())
                    emitWarning("Actor type '{0}' grants conditions that are not consumed:{1} ".F(actorInfo.Key, unconsumed.JoinWith(", ")));

                var ungranted = consumed.Except(granted);
                if (ungranted.Any())
                    emitError("Actor type '{0}' consumes conditions that are not granted:{1}".F(actorInfo.Key, ungranted.JoinWith(", ")));

                if ((consumed.Any() || granted.Any()) && actorInfo.Value.TraitInfoOrDefault<ConditionManagerInfo>() == null)
                    emitError("Actor type '{0}' defines conditions bute does not include a ConditionManager".F(actorInfo.Key));
            }
        }

    }
}