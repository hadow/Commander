using System;
using System.IO;
using System.Linq;
using EW.Scripting;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("General")]   
    public class ConditionProperties:ScriptActorProperties,Requires<ExternalConditionInfo>
    {

        readonly ExternalCondition[] externalConditions;


        public ConditionProperties(ScriptContext context,Actor self):base(context,self)
        {
            externalConditions = self.TraitsImplementing<ExternalCondition>().ToArray();


        }

        /// <summary>
        /// Check whether this actor accepts a specific external condition.
        /// </summary>
        /// <returns><c>true</c>, if condition was acceptsed, <c>false</c> otherwise.</returns>
        /// <param name="condition">Condition.</param>
        public bool AcceptsCondition(string condition){

            return externalConditions.Any(t => t.Info.Condition == condition && t.CanGrantCondition(Self, this));
        }


        public int GrantCondition(string condition,int duration = 0){

            var external = externalConditions.FirstOrDefault(t => t.Info.Condition == condition && t.CanGrantCondition(Self, this));

            if (external == null)
                throw new InvalidDataException("Condition '{0}'  has not been listed  on an enabled ExternalCondition tait".F(condition));

            return external.GrantCondition(Self, this, duration);
        }
    }
}
