using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public abstract class UpgradableTraitInfo : IUpgradableInfo
    {

        public readonly HashSet<string> UpgradeTypes = new HashSet<string>();

        public readonly int UpgradeMinEnabledLevel = 0;

        public readonly int UpgradeMaxEnabledLevel = int.MaxValue;

        public readonly int UpgradeMaxAcceptedLevel = 1;

        public abstract object Create(ActorInitializer init);
    }



    public abstract class UpgradableTrait<T>:IUpgradable,IDisabledTrait where T:UpgradableTraitInfo
    {

        public readonly T Info;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="oldLevel"></param>
        /// <param name="newLevel"></param>
        protected virtual void UpgradeLevelChanged(Actor self,int oldLevel,int newLevel) { }

        protected virtual void UpgradeEnabled(Actor self) { }

        protected virtual void UpgradeDisabled(Actor self) { }
    }
}