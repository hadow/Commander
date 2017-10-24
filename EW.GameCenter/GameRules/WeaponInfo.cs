using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Effects;
namespace EW
{

    public class ProjectileArgs
    {
        public WeaponInfo Weapon;
        public int[] DamagedModifiers;

        /// <summary>
        /// 不精确
        /// </summary>
        public int[] InaccuracyModifiers;

        public int[] RangeModifiers;

        public int Facing;

        public WPos Source;

        public Func<WPos> CurrentSource;

        public Actor SourceActor;
        /// <summary>
        /// 被动目标
        /// </summary>
        public WPos PassiveTarget;

        /// <summary>
        /// 指定目标
        /// </summary>
        public Target GuidedTarget;
    }

    public interface IProjectile:IEffect { }
    public interface IProjectileInfo { IProjectile Create(ProjectileArgs args); }
    public sealed class WeaponInfo
    {
        /// <summary>
        /// 武器可以射击的最大范围
        /// </summary>
        public readonly WDist Range = WDist.Zero;

        public readonly WDist MinRange = WDist.Zero;

        /// <summary>
        /// The sound played when the weapon is fired.
        /// </summary>
        public readonly string[] Report = null;

        /// <summary>
        /// 重新装载弹药之间的延迟
        /// </summary>
        public readonly int ReloadDelay = 1;

        /// <summary>
        /// 
        /// </summary>
        public readonly HashSet<string> ValidTargets = new HashSet<string> { "Gound", "Water" };

        /// <summary>
        /// 
        /// </summary>
        public readonly HashSet<string> InvalidTargets = new HashSet<string>();

        public readonly int Burst = 1;

        public readonly int BurstDelay = 5;

        [FieldLoader.LoadUsing("LoadProjectile")]
        public readonly IProjectileInfo Projectile;

        [FieldLoader.LoadUsing("LoadWarheads")]
        public readonly List<IWarHead> Warheads = new List<IWarHead>();

        /// <summary>
        /// Does the weapon aim at the target's center regardless of other targetable offsets?
        /// </summary>
        public readonly bool TargetActorCenter = false;
        public WeaponInfo(string name, MiniYaml content)
        {
            FieldLoader.Load(this, content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>
        static object LoadProjectile(MiniYaml yaml)
        {
            MiniYaml proj;
            if (!yaml.ToDictionary().TryGetValue("Projectile", out proj))
                return null;

            var ret = WarGame.CreateObject<IProjectileInfo>(proj.Value + "Info");
            FieldLoader.Load(ret, proj);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yaml"></param>
        /// <returns></returns>
        static object LoadWarheads(MiniYaml yaml)
        {
            var retList = new List<IWarHead>();

            foreach(var node in yaml.Nodes.Where(n => n.Key.StartsWith("Warhead")))
            {
                var ret = WarGame.CreateObject<IWarHead>(node.Value.Value + "Warhead");
                FieldLoader.Load(ret, node.Value);
                retList.Add(ret);
            }

            return retList;
        }

        public void Impact(Target target,Actor firedBy,IEnumerable<int> damageModifiers)
        {

        }

        /// <summary>
        /// 目标类型是否通过验证
        /// </summary>
        /// <param name="targetTypes"></param>
        /// <returns></returns>
        public bool IsValidTarget(IEnumerable<string> targetTypes)
        {
            return ValidTargets.Overlaps(targetTypes) && !InvalidTargets.Overlaps(targetTypes);
        }

        public bool IsValidAgainst(Target target,World world,Actor firedBy)
        {
            if(target.Type == TargetT.Actor)
            {
                return IsValidAgainst(target.Actor, firedBy);
            }

            if(target.Type == TargetT.FrozenActor)
            {
                return IsValidAgainst(target.FrozenActor, firedBy);
            }

            if(target.Type == TargetT.Terrain)
            {
                var cell = world.Map.CellContaining(target.CenterPosition);
                if (!world.Map.Contains(cell))
                    return false;

                var cellInfo = world.Map.GetTerrainInfo(cell);
                if (!IsValidTarget(cellInfo.TargetTypes))
                    return false;
            }

            return false;
        }

        public bool IsValidAgainst(Actor victim,Actor firedBy)
        {
            var targetTypes = victim.GetEnabledTargetTypes();

            if (!IsValidTarget(targetTypes))
                return false;

            //PERF:Avoid LINQ;
            foreach(var warhead in Warheads)
            {
                if (warhead.IsValidAgainst(victim, firedBy))
                    return true;
            }
            return false;
        }


        public bool IsValidAgainst(FrozenActor victim,Actor firedBy)
        {
            if (!IsValidTarget(victim.TargetTypes))
                return false;

            if (!Warheads.Any(w => w.IsValidAgainst(victim, firedBy)))
                return false;

            return true;
        }

    }
}