using System;
using System.Collections.Generic;
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
    /// </summary>
    public class CloakInfo : ConditionalTraitInfo
    {
        public readonly int InitialDelay = 10;

        public readonly int CloakDelay = 30;

        public readonly UncloakType UncloakOn = UncloakType.Attack | UncloakType.Unload | UncloakType.Infiltrate | UncloakType.Demolish | UncloakType.Dock;

        public readonly string CloakSound = null;
        public readonly string UncloakSound = null;

        public readonly HashSet<string> CloakTypes = new HashSet<string> { "Cloak" };

        [PaletteReference("IsPlayerPalette")]
        public readonly string Palette = "cloak";
        public readonly bool IsPlayerPalette = false;

        public readonly string CloakedCondition = null;


        public override object Create(ActorInitializer init)
        {
            return new Cloak(this);
        }
    }




    public class Cloak:ConditionalTrait<CloakInfo>,IRenderModifier,INotifyAttack,ITick
    {
        [Sync]
        int remainingTime;

        bool isDocking;
        ConditionManager conditionManager;

        CPos? lastPos;

        bool wasCloaked = false;

        bool firstTick = true;

        int cloakedToken = ConditionManager.InvalidConditionToken;

        public Cloak(CloakInfo info) : base(info)
        {
            remainingTime = info.InitialDelay;
        }

        void ITick.Tick(Actor self)
        {
            if (!IsTraitDisabled)
            {
                if (remainingTime > 0 && !isDocking)
                    remainingTime--;

                if (self.IsDisabled())
                    Uncloak();

            }
        }

        public bool Cloaked
        {
            get { return !IsTraitDisabled && remainingTime <= 0; }
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
                var palette = string.IsNullOrEmpty(Info.pa)
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
    }
}