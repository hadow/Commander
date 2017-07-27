using System;
using EW.Scripting;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Scripting
{
    [ScriptPropertyGroup("General")]
    public class HealthProperties : ScriptActorProperties, Requires<HealthInfo>
    {
        Health health;

        public HealthProperties(ScriptContext context,Actor self) : base(context, self)
        {
            health = self.Trait<Health>();
        }


        public int Health
        {
            get { return health.HP; }
            set
            {
                health.InflictDamage(Self, Self, health.HP - value, null, true);
            }
        }

        public int MaxHealth { get { return health.MaxHP; } }

        public void Kill()
        {
            health.InflictDamage(Self, Self, health.MaxHP, null, true);
        }


    }
}