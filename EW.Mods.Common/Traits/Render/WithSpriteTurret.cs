using System;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
using System.Linq;
using System.Collections.Generic;
namespace EW.Mods.Common.Traits.Render
{
    public class WithSpriteTurretInfo : ConditionalTraitInfo,Requires<RenderSpritesInfo>,Requires<TurretedInfo>,Requires<BodyOrientationInfo>,Requires<ArmamentInfo>
    {

        [SequenceReference]
        public readonly string Sequence = "turret";

        /// <summary>
        /// Sequence name to use when prepared to fire.
        /// </summary>
        [SequenceReference]public readonly string AnimSequence = null;

        /// <summary>
        /// Turreted 'Turret' key to display
        /// </summary>
        public readonly string Turret = "primary";

        /// <summary>
        /// Render recoil
        /// </summary>
        public readonly bool Recoils = true;

        public override object Create(ActorInitializer init)
        {
            return new WithSpriteTurret(init.Self, this);
        }
    }
    public class WithSpriteTurret:ConditionalTrait<WithSpriteTurretInfo>,INotifyBuildComplete,ITick,INotifyDamageStateChanged
    {
        public readonly Animation DefaultAnimation;
        protected readonly AttackBase Attack;

        readonly RenderSprites rs;
        readonly BodyOrientation body;
        readonly Turreted t;
        readonly Armament[] arms;

        bool buildComplete;


        public WithSpriteTurret(Actor self,WithSpriteTurretInfo info) : base(info)
        {

            rs = self.Trait<RenderSprites>();
            body = self.Trait<BodyOrientation>();
            Attack = self.TraitOrDefault<AttackBase>();
            t = self.TraitsImplementing<Turreted>().First(tt => tt.Name == Info.Turret);

            arms = self.TraitsImplementing<Armament>().Where(w => w.Info.Turret == info.Turret).ToArray();

            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();

            DefaultAnimation = new Animation(self.World, rs.GetImage(self), () => t.TurretFacing);
            DefaultAnimation.PlayRepeating(NormalizeSequence(self, info.Sequence));
            rs.Add(new AnimationWithOffset(DefaultAnimation,
                () => TurretOffset(self),
                () => IsTraitDisabled || !buildComplete,
                p => RenderUtils.ZOffsetFromCenter(self, p, 1)));

            t.QuantizedFacings = DefaultAnimation.CurrentSequence.Facings;
        }

        void ITick.Tick(Actor self)
        {
            Tick(self);
        }

        protected virtual void Tick(Actor self)
        {
            if (Info.AnimSequence == null)
                return;

            var sequence = Attack.IsAniming ? Info.AnimSequence : Info.Sequence;
            DefaultAnimation.ReplaceAnim(sequence);
        }

        void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo attackInfo)
        {
            DamageStateChanged(self);
        }

        protected virtual void DamageStateChanged(Actor self)
        {
            if (DefaultAnimation.CurrentSequence != null)
                DefaultAnimation.ReplaceAnim(NormalizeSequence(self, DefaultAnimation.CurrentSequence.Name));
        }


        void INotifyBuildComplete.BuildingComplete(Actor self)
        {
            buildComplete = true;
        }
        protected virtual WVec TurretOffset(Actor self)
        {
            if (!Info.Recoils)
                return t.Position(self);

            var recoil = arms.Aggregate(WDist.Zero, (a, b) => a + b.Recoil);
            var localOffset = new WVec(-recoil, WDist.Zero, WDist.Zero);
            var quantizedWorldTurret = t.WorldOrientation(self);
            return t.Position(self) + body.LocalToWorld(localOffset.Rotate(quantizedWorldTurret));

        }


        public string NormalizeSequence(Actor self,string sequence)
        {
            return RenderSprites.NormalizeSequence(DefaultAnimation, self.GetDamageState(), sequence);
        }
    }
}