using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public abstract class UpgradableTraitInfo : IUpgradableInfo
    {
        [UpgradeUsedReference]
        public readonly HashSet<string> UpgradeTypes = new HashSet<string>();

        public readonly int UpgradeMinEnabledLevel = 0;

        public readonly int UpgradeMaxEnabledLevel = int.MaxValue;

        /// <summary>
        /// The maximum upgrade level that this trait will accept.
        /// </summary>
        public readonly int UpgradeMaxAcceptedLevel = 1;

        public abstract object Create(ActorInitializer init);
    }


    /// <summary>
    /// Abstract base for enabling and disabling trait using upgrades.
    /// Requires basing *Info on UpgradableTraitInfo and using base(info) constructor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UpgradableTrait<T>:IUpgradable,IDisabledTrait,ISync where T:UpgradableTraitInfo
    {

        public readonly T Info;
        [Sync]
        public bool IsTraitDisabled { get; private set; }

        public IEnumerable<string> UpgradeTypes { get { return Info.UpgradeTypes; } }

        public UpgradableTrait(T info)
        {
            Info = info;
            IsTraitDisabled = info.UpgradeTypes != null && info.UpgradeTypes.Count > 0 && info.UpgradeMinEnabledLevel > 0;
        }
        public bool AcceptsUpgradeLevel(Actor self,string type,int level)
        {
            return level > 0 && level <= Info.UpgradeMaxAcceptedLevel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="type"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        public void UpgradeLevelChanged(Actor self,string type,int oldLevel,int newLevel)
        {
            if (!Info.UpgradeTypes.Contains(type))
                return;

            //Restrict the levels to the allowed range
            oldLevel = oldLevel.Clamp(0, Info.UpgradeMaxAcceptedLevel);
            newLevel = newLevel.Clamp(0, Info.UpgradeMaxAcceptedLevel);

            if (oldLevel == newLevel)
                return;

            var wasDisabled = IsTraitDisabled;
            IsTraitDisabled = newLevel < Info.UpgradeMinEnabledLevel || newLevel > Info.UpgradeMaxEnabledLevel;
            UpgradeLevelChanged(self, oldLevel, newLevel);

            if(IsTraitDisabled != wasDisabled)
            {
                if (wasDisabled)
                    UpgradeEnabled(self);
                else
                    UpgradeDisabled(self);

            }
        }

        //Subclasses can add upgrade support by querying IsTraitDisables and/or overriding these methods
        protected virtual void UpgradeLevelChanged(Actor self,int oldLevel,int newLevel) { }

        protected virtual void UpgradeEnabled(Actor self) { }

        protected virtual void UpgradeDisabled(Actor self) { }
    }
}