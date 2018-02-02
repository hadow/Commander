using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    public enum UncloakType
    {
        None = 0,
        Attack = 1,
        Move = 2,
        Unload = 4,
        Infiltrate = 8,//潜入
        Demolish = 16,//拆除，破坏
        Damage = 32,
        Heal = 64,
        SelfHeal = 128,
        Dock = 256,//码头,船坞
    }


    /// <summary>
    /// This unit can be cloak and uncloak in specific situations.
    /// 隱蔽
    /// </summary>
    public class CloakInfo : ConditionalTraitInfo
    {
        public readonly int InitialDelay = 10;

        public readonly int CloakDelay = 30;

        /// <summary>
        /// Events leading to the actor getting uncloaked.
        /// Possible values are :Attack,Move,Unload,Infiltrate,Demolish,Dock,Damage,Heal and SelfHeal.
        /// </summary>
        public readonly UncloakType UncloakOn = UncloakType.Attack | UncloakType.Unload | UncloakType.Infiltrate | UncloakType.Demolish | UncloakType.Dock;


        public readonly string CloakSound = null;
        public readonly string UncloakSound = null;

        public readonly HashSet<string> CloakTypes = new HashSet<string> { "Cloak" };

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = "cloak";

        public readonly bool IsPlayerPalette = false;

        /// <summary>
        /// The condition to grant to self while cloaked.
        /// </summary>
        [GrantedConditionReference]
        public readonly string CloakedCondition = null;


        public override object Create(ActorInitializer init)
        {
            return new Cloak(this);
        }
    }




    public class Cloak:ConditionalTrait<CloakInfo>,IRenderModifier,
        INotifyCreated,
        INotifyDamage,
        INotifyAttack,
        ITick,
        IVisibilityModifier,
        IRadarColorModifier,
        INotifyHarvesterAction,
        INotifyUnload

    {
        [Sync]
        int remainingTime;

        bool isDocking;
        ConditionManager conditionManager;

        CPos? lastPos;

        Cloak[] otherCloaks;


        bool wasCloaked = false;

        bool firstTick = true;

        int cloakedToken = ConditionManager.InvalidConditionToken;

        public Cloak(CloakInfo info) : base(info)
        {
            remainingTime = info.InitialDelay;
        }

        public bool Cloaked{ get { return !IsTraitDisabled && remainingTime <= 0; }}

        protected override void Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();
            otherCloaks = self.TraitsImplementing<Cloak>().Where(c => c != this).ToArray();

            if(Cloaked)
            {
                wasCloaked = true;

                if (conditionManager != null && cloakedToken == ConditionManager.InvalidConditionToken && !string.IsNullOrEmpty(Info.CloakedCondition))
                    cloakedToken = conditionManager.GrantCondition(self, Info.CloakedCondition);
                
            }

            base.Created(self);
        }

        void ITick.Tick(Actor self)
        {
            if (!IsTraitDisabled)
            {
                if (remainingTime > 0 && !isDocking)
                    remainingTime--;
                

                if(Info.UncloakOn.HasFlag(UncloakType.Move) && (lastPos == null || lastPos.Value != self.Location))
                {
                    Uncloak();
                    lastPos = self.Location;
                }
            }

            var isCloaked = Cloaked;

            if(isCloaked && !wasCloaked)
            {
                if (conditionManager != null && cloakedToken == ConditionManager.InvalidConditionToken && !string.IsNullOrEmpty(Info.CloakedCondition))
                    cloakedToken = conditionManager.GrantCondition(self, Info.CloakedCondition);

                //Sounds shouldn't play if the actor starts cloaked.
                if (!(firstTick && Info.InitialDelay == 0) && !otherCloaks.Any(a => a.Cloaked))
                    WarGame.Sound.Play(SoundType.World, Info.CloakSound, self.CenterPosition);

            }
            else if(!isCloaked && wasCloaked)
            {
                if (cloakedToken != ConditionManager.InvalidConditionToken)
                    cloakedToken = conditionManager.RevokeCondition(self, cloakedToken);

                if (!(firstTick && Info.InitialDelay == 0) && !otherCloaks.Any(a => a.Cloaked))
                    WarGame.Sound.Play(SoundType.World, Info.UncloakSound, self.CenterPosition);
            }

            wasCloaked = isCloaked;
            firstTick = false;
        }


        public void Uncloak()
        {
            Uncloak(Info.CloakDelay);
        }

        public void Uncloak(int time)
        {
            remainingTime = Math.Max(remainingTime, time);
        }

        IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
        {
            if (remainingTime > 0 || IsTraitDisabled)
                return r;

            if(Cloaked && IsVisible(self, self.World.RenderPlayer))
            {
                var palette = string.IsNullOrEmpty(Info.Palette) ? null : Info.IsPlayerPalette ? wr.Palette(Info.Palette + self.Owner.InternalName) : wr.Palette(Info.Palette);

                if (palette == null)
                    return r;
                else
                    return r.Select(a => a.IsDecoration ? a : a.WithPalette(palette));
            }
            else
            {
                return SpriteRenderable.None;
            }
        }

        public bool IsVisible(Actor self,Player viewer)
        {
            if(!Cloaked || self.Owner.IsAlliedWith(viewer))
            {
                return true;
            }

            return self.World.ActorsWithTrait<DetectCloaked>().Any(a => !a.Trait.IsTraitDisabled
            && a.Actor.Owner.IsAlliedWith(viewer)
            && Info.CloakTypes.Overlaps(a.Trait.Info.CloakTypes)
            && (self.CenterPosition - a.Actor.CenterPosition).LengthSquared <= a.Trait.Info.Range.LengthSquared);
        }

        protected override void TraitDisabled(Actor self)
        {
            Uncloak();
        }

        void INotifyAttack.Attacking(Actor self, Target target, Armament a, Barrel barrel)
        {
            if (Info.UncloakOn.HasFlag(UncloakType.Attack))
                Uncloak();
        }

        void INotifyAttack.PreparingAttack(Actor self, Target target, Armament a, Barrel barrel){}

        IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, WorldRenderer wr, IEnumerable<Rectangle> bounds)
        {
            return bounds;
        }


        /// <summary>
        /// 受到伤害时取消隐藏
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attackInfo"></param>
        void INotifyDamage.Damaged(Actor self,AttackInfo attackInfo){
            if (attackInfo.Damage.Value == 0)
                return;

            var type = attackInfo.Damage.Value < 0 ? (attackInfo.Attacker == self ? UncloakType.SelfHeal : UncloakType.Heal) : UncloakType.Damage;

            if (Info.UncloakOn.HasFlag(type))
                Uncloak();
        }

        Color IRadarColorModifier.RadarColorOverride(Actor self, Color color)
        {
            if (self.Owner == self.World.LocalPlayer && Cloaked)
                color = Color.FromArgb(128, color);
            return color;
        }

        void INotifyHarvesterAction.Docked()
        {
            if(Info.UncloakOn.HasFlag(UncloakType.Dock))
            {
                isDocking = true;
                Uncloak();
            }
        }

        void INotifyHarvesterAction.Undocked()
        {
            isDocking = false;
        }

        void INotifyHarvesterAction.Harvested(Actor self, ResourceType resource) { }

        void INotifyUnload.Unloading(Actor self)
        {
            if (Info.UncloakOn.HasFlag(UncloakType.Unload))
                Uncloak();
        }

    }
}