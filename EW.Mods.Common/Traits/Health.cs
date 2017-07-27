using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.HitShapes;
namespace EW.Mods.Common.Traits
{

    public class HealthInfo : ITraitInfo,UsesInit<HealthInit>
    {
        public readonly int HP = 0;

        public readonly bool NotifyAppliedDamage = true;


        [FieldLoader.LoadUsing("LoadShape")]
        public readonly IHitShape Shape;


        static object LoadShape(MiniYaml yaml)
        {
            IHitShape ret;

            var shapeNode = yaml.Nodes.Find(n => n.Key == "Shape");
            var shape = shapeNode != null ? shapeNode.Value.Value : string.Empty;

            if(!string.IsNullOrEmpty(shape))
            {
                ret = WarGame.CreateObject<IHitShape>(shape + "Shape");
                try
                {
                    FieldLoader.Load(ret, shapeNode.Value);
                }
                catch(YamlException e)
                {
                    throw new YamlException("HitShape {0}:{1}".F(shape, e.Message));
                }
            }
            else
            {
                ret = new CircleShape();
            }
            ret.Initialize();
            return ret;
        }

        public virtual object Create(ActorInitializer init)
        {
            return new Health(init,this);
        }
    }
    public class Health:IHealth,ISync,ITick
    {
        public readonly HealthInfo Info;

        [Sync]
        int hp;

        public int HP { get { return hp; } }

        public int MaxHP { get; private set; }
        public int DisplayHP { get; private set; }

        public bool IsDead { get { return hp <= 0; } }

        public bool RemoveOnDeath;

        public Health(ActorInitializer init,HealthInfo info)
        {
            Info = info;
            MaxHP = info.HP > 0 ? info.HP : 1;

            hp = init.Contains<HealthInit>() ? init.Get<HealthInit, int>() * MaxHP / 100 : MaxHP;

            DisplayHP = hp;
        }


        public DamageState DamageState
        {
            get
            {
                if (hp <= 0)
                    return DamageState.Dead;


                if (hp == MaxHP)
                    return DamageState.Undamaged;

                return DamageState.Light;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="attacker"></param>
        /// <param name="damage"></param>
        /// <param name="warhead"></param>
        /// <param name="ignoreModifiers"></param>
        public void InflictDamage(Actor self,Actor attacker,int damage,IWarHead warhead,bool ignoreModifiers)
        {
            if (IsDead)
                return;

            var oldState = DamageState;
            if(!ignoreModifiers && damage > 0)
            {

            }
        }

        public void Kill(Actor self,Actor attacker)
        {

        }


        public void Tick(Actor self)
        {

        }

        /// <summary>
        /// 复活
        /// </summary>
        /// <param name="self"></param>
        /// <param name="repairer"></param>
        public void Resurrect(Actor self,Actor repairer)
        {

        }

    }

    public  class HealthInit : IActorInit<int>
    {
        [FieldLoader.FieldFromYamlKey]
        readonly int value = 100;

        readonly bool allowZero;

        public HealthInit() { }

        public HealthInit(int init,bool allowZero = false)
        {
            this.allowZero = allowZero;
            value = init;
        }

        public int Value(World world)
        {
            if (value < 0 || (value == 0 && !allowZero))
                return 1;
            return value;
        }
    }
}