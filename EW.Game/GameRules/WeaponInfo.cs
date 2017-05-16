using System;
using System.Collections.Generic;
using EW.Traits;
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

    public interface Iprojectile { }
    public interface IProjectileInfo { Iprojectile Create(ProjectileArgs args); }
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

        public WeaponInfo(string name, MiniYaml content)
        {
            FieldLoader.Load(this, content);
        }

        static object LoadProjectile(MiniYaml yaml)
        {
            MiniYaml proj;
            if (!yaml.ToDictionary().TryGetValue("Projectile", out proj))
                return null;
        }

        public void Impact(Target target,Actor firedBy,IEnumerable<int> damageModifiers)
        {

        }


    }
}