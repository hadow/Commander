using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public abstract class AttackBaseInfo:UpgradableTraitInfo
    {
        public readonly string[] Armaments = { "primary", "secondary" };

        public readonly string Cursor = null;

        public readonly string OutsideRangeCursor = null;

        public readonly bool AttackRequiresEnteringCell = false;

        public readonly bool IgnoresVisibility = false;

        public readonly string Voice = "Action";

        public abstract override object Create(ActorInitializer init);

    }
    public abstract class AttackBase:UpgradableTrait<AttackBaseInfo>
    {
        readonly string attackOrderName = "Attack";
        readonly string forceAttackOrderName = "ForceAttack";

        [Sync]
        public bool IsAttacking { get; internal set; }

        protected Lazy<IFacing> facing;
        protected Lazy<Building> building;
        protected Lazy<IPositionable> positionable;

        readonly Actor self;

        public AttackBase(Actor self,AttackBaseInfo info) : base(info)
        {
            this.self = self;

            facing = Exts.Lazy(() => self.TraitOrDefault<IFacing>());
            building = Exts.Lazy(() => self.TraitOrDefault<Building>());
            positionable = Exts.Lazy(() => self.TraitOrDefault<IPositionable>());
        }
    }
}