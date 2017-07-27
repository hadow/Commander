using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class UpgradeMultiplierTraitInfo : ITraitInfo
    {

        [UpgradeUsedReference]
        public readonly string[] UpgradeTypes = { };

        public readonly int BaseLevel = 1;

        [FieldLoader.Require]
        public readonly int[] Modifier = { };


        public abstract object Create(ActorInitializer init);
    }

    public abstract class UpgradeMultiplierTrait:IUpgradable,IDisabledTrait,ISync
    {
        readonly UpgradeMultiplierTraitInfo info;
        public IEnumerable<string> UpgradeTypes { get{ return info.UpgradeTypes; } }

        [Sync]
        int level = 0;

        [Sync]
        public bool IsTraitDisabled { get; private set; }

        protected UpgradeMultiplierTrait(UpgradeMultiplierTraitInfo info,string modifierType,string actorType)
        {
            if (info.Modifier.Length == 0)
                throw new Exception("No modifiers in " + modifierType + " for " + actorType);

            this.info = info;
            IsTraitDisabled = info.UpgradeTypes.Length > 0 && info.BaseLevel > 0;
            level = IsTraitDisabled ? 0 : info.BaseLevel;
        }
        public bool AcceptsUpgradeLevel(Actor self,string type,int level)
        {
            return level < info.Modifier.Length + info.BaseLevel;
        }

        public void UpgradeLevelChanged(Actor self,string type,int oldLevel,int newLevel)
        {
            if (!UpgradeTypes.Contains(type))
                return;

            level = newLevel.Clamp(0, Math.Max(info.Modifier.Length + info.BaseLevel - 1, 0));
            IsTraitDisabled = level < info.BaseLevel;
            Update(self);
        }

        protected virtual void Update(Actor self) { }

        public int GetModifier()
        {
            return IsTraitDisabled ? 100 : info.Modifier[level - info.BaseLevel];
        }
    }
}