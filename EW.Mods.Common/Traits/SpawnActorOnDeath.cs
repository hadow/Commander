using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    public enum OwnerType { Victim,Killer,InternalName}

    /// <summary>
    /// Spawn another actor immediately upon death.
    /// </summary>
    public class SpawnActorOnDeathInfo : ConditionalTraitInfo
    {
        [ActorReference,FieldLoader.Require]
        public readonly string Actor = null;

        /// <summary>
        /// Probability the actor spawns.
        /// </summary>
        public readonly int Probability = 100;

        public readonly OwnerType OwnerType = OwnerType.Victim;

        public readonly string InternalOwner = "Neutral";

        public readonly bool EffectiveOwnerFromOwner = false;
        /// <summary>
        /// DeathType that triggers the actor spawn.
        /// </summary>
        public readonly string DeathType = null;

        public readonly bool RequiresLobbyCreeps = false;

        /// <summary>
        /// Skips the spawned actor's make animation if true.
        /// </summary>
        public readonly bool SkipMakeAnimations = true;

        public readonly CVec Offset = CVec.Zero;

        public readonly bool SpawnAfterDefeat = true;

        public override object Create(ActorInitializer init)
        {
            return new SpawnActorOnDeath(init, this);
        }
    }
    public class SpawnActorOnDeath:ConditionalTrait<SpawnActorOnDeathInfo>,INotifyKilled,INotifyRemovedFromWorld
    {
        readonly string faction;
        readonly bool enabled;

        Player attackingPlayer;


        public SpawnActorOnDeath(ActorInitializer init,SpawnActorOnDeathInfo info) : base(info)
        {
            enabled = !info.RequiresLobbyCreeps || init.Self.World.WorldActor.Trait<MapCreeps>().Enabled;
            faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : init.Self.Owner.Faction.InternalName;

        }

        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {
            if (!enabled || IsTraitDisabled)
                return;

            if (!self.IsInWorld)
                return;

            if (self.World.SharedRandom.Next(100) > Info.Probability)
                return;

            if (Info.DeathType != null && !attackInfo.Damage.DamageTypes.Contains(Info.DeathType))
                return;

            attackingPlayer = attackInfo.Attacker.Owner;



        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            if (attackingPlayer == null)
                return;

            var defeated = self.Owner.WinState == WinState.Lost;

            if (defeated && !Info.SpawnAfterDefeat)
                return;

            var td = new TypeDictionary
            {
                new ParentActorInit(self),
                new LocationInit(self.Location+Info.Offset),
                new CenterPositionInit(self.CenterPosition),
                new FactionInit(faction)
            };

            if (self.EffectiveOwner != null && self.EffectiveOwner.Disguised)
                td.Add(new EffectiveOwnerInit(self.EffectiveOwner.Owner));
            else if (Info.EffectiveOwnerFromOwner)
                td.Add(new EffectiveOwnerInit(self.Owner));

            if (Info.OwnerType == OwnerType.Victim)
            {
                if (!defeated || string.IsNullOrEmpty(Info.InternalOwner))
                    td.Add(new OwnerInit(self.Owner));
                else
                {
                    td.Add(new OwnerInit(self.World.Players.First(p => p.InternalName == Info.InternalOwner)));
                    if (!td.Contains<EffectiveOwnerInit>())
                        td.Add(new EffectiveOwnerInit(self.Owner));
                }
            }
            else if (Info.OwnerType == OwnerType.Killer)
                td.Add(new OwnerInit(attackingPlayer));
            else
                td.Add(new OwnerInit(self.World.Players.First(p => p.InternalName == Info.InternalOwner)));

            if (Info.SkipMakeAnimations)
                td.Add(new SkipMakeAnimsInit());

            foreach (var modifier in self.TraitsImplementing<IDeathActorInitModifier>())
                modifier.ModifyDeathActorInit(self, td);

            var huskActor = self.TraitsImplementing<IHuskModifier>().Select(ihm => ihm.HuskActor(self)).FirstOrDefault(a => a != null);

            self.World.AddFrameEndTask(w => w.CreateActor(huskActor ?? Info.Actor, td));
        }
    }
}