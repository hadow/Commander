using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Warheads
{
    public class CreateEffectWarhead:Warhead,IRulesetLoaded<WeaponInfo>
    {
        /// <summary>
        /// List of explosion sequences that can be used.
        /// </summary>
        [SequenceReference("Image")]
        public readonly string[] Explosions = new string[0];

        public readonly string Image = "explosion";

        public readonly string ExplosionPalette = "effect";


        public readonly bool UsePlayerPalette = false;


        public readonly bool ForceDisplayAtGroundLevel = false;

        public readonly string[] ImpactSounds = new string[0];


        public readonly int ImpactSoundChance = 100;

        public readonly WDist AirThreshold = new WDist(128);

        public WDist VictimScanRadius = new WDist(-1);


        static readonly string[] TargetTypeAir = new string[] { "Air" };
        void IRulesetLoaded<WeaponInfo>.RulesetLoaded(Ruleset rules, WeaponInfo info)
        {
            if (VictimScanRadius < WDist.Zero)
                VictimScanRadius = Util.MinimumRequiredBlockerScanRadius(rules);
        }


        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers)
        {

            if (!target.IsValidFor(firedBy))
                return;

            var pos = target.CenterPosition;
            var world = firedBy.World;
            var targetTile = world.Map.CellContaining(pos);
            var isValid = IsValidImpact(pos, firedBy);

            if ((!world.Map.Contains(targetTile)) || (!isValid))
                return;

            var palette = ExplosionPalette;
            if (UsePlayerPalette)
                palette += firedBy.Owner.InternalName;

            var explosion = Explosions.RandomOrDefault(WarGame.CosmeticRandom);

            if(Image != null && explosion != null)
            {
                if (ForceDisplayAtGroundLevel)
                {
                    var dat = world.Map.DistanceAboveTerrain(pos);
                    pos = new WPos(pos.X, pos.Y, pos.Z - dat.Length);
                }

                world.AddFrameEndTask(w => w.Add(new SpriteEffect(pos, w, Image, explosion, palette)));
            }

            var impactSound = ImpactSounds.RandomOrDefault(WarGame.CosmeticRandom);
            if (impactSound != null && WarGame.CosmeticRandom.Next(0, 100) < ImpactSoundChance)
                WarGame.Sound.Play(SoundType.World, impactSound, pos);
        }


        public bool IsValidImpact(WPos pos,Actor firedBy)
        {
            var world = firedBy.World;
            var targetTile = world.Map.CellContaining(pos);
            if (!world.Map.Contains(targetTile))
                return false;

            var impactType = GetImpactType(world, targetTile, pos, firedBy);
            switch (impactType)
            {
                case ImpactType.TargetHit:
                    return true;
                case ImpactType.Air:
                    return IsValidTarget(TargetTypeAir);
                case ImpactType.Ground:
                    var tileInfo = world.Map.GetTerrainInfo(targetTile);
                    return IsValidTarget(tileInfo.TargetTypes);
                default:
                    return false;
            }
        }

        public ImpactType GetImpactType(World world,CPos cell,WPos pos,Actor firedBy)
        {
            if(VictimScanRadius > WDist.Zero)
            {
                var targetType = GetDirectHitTargetType(world, cell, pos, firedBy, true);
                if (targetType == ImpactTargetType.ValidActor)
                    return ImpactType.TargetHit;
                if (targetType == ImpactTargetType.InvalidActor)
                    return ImpactType.None;
            }

            var dat = world.Map.DistanceAboveTerrain(pos);
            if (dat > AirThreshold)
                return ImpactType.Air;

            return ImpactType.Ground;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cell"></param>
        /// <param name="pos"></param>
        /// <param name="firedBy"></param>
        /// <param name="checkTargetValidity">检查目标有效性</param>
        /// <returns></returns>
        public ImpactTargetType GetDirectHitTargetType(World world,CPos cell,WPos pos,Actor firedBy,bool checkTargetValidity = false)
        {
            var victims = world.FindActorsInCircle(pos, VictimScanRadius);
            var invalidHit = false;

            foreach(var victim in victims)
            {
                if (!AffectsParent && victim == firedBy)
                    continue;

                if (!victim.Info.HasTraitInfo<HealthInfo>())
                    continue;

                var activeShapes = victim.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
                var directHit = activeShapes.Any(i => i.Info.Type.DistanceFromEdge(pos, victim).Length <= 0);

                //If the warhead landed outside the actor's hit-shape(s),we need to skip the rest so it won't be considered an invalidHit.
                if (!directHit)
                    continue;

                if (!checkTargetValidity || IsValidAgainst(victim, firedBy))
                    return ImpactTargetType.ValidActor;


                //If we got here,it must be an invalid target.
                invalidHit = true;
            }
            //If there was at least at a single direct hit,bue none on valid target(s),we return InvalidActor.
            return invalidHit ? ImpactTargetType.InvalidActor : ImpactTargetType.NoActor;

        }




    }
}