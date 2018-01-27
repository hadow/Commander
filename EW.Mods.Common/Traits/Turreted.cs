using System;
using System.Linq;
using EW.Traits;
using EW.Primitives;
using System.Collections.Generic;
namespace EW.Mods.Common.Traits
{

    public class TurretedInfo : ITraitInfo,UsesInit<TurretFacingInit>,Requires<BodyOrientationInfo>
    {
        public readonly string Turret = "primary";

        public readonly int TurnSpeed = 255;

        public readonly int InitialFacing = 0;

        public readonly int RealignDelay = 40;


        public readonly WVec Offset = WVec.Zero;

        public readonly int PreviewFacing = 92;


        public virtual object Create(ActorInitializer init) { return new Turreted(init,this); }
    }
    public class Turreted:ITick,INotifyCreated,ISync,IDeathActorInitModifier
    {
        public readonly TurretedInfo Info;

        AttackTurreted attack;
        IFacing facing;
        BodyOrientation body;

        [Sync]
        public int QuantizedFacings = 0;
        [Sync]
        public int TurretFacing = 0;

        public int? DesiredFacing;

        int realignTick = 0;

        //For subclasses that want to move the turret relative to the body.
        protected WVec localOffset = WVec.Zero;

        public WVec Offset { get { return Info.Offset + localOffset; } }


        public string Name { get { return Info.Turret; } }


        public static Func<int> TurretFacingFromInit(IActorInitializer init,int def,string turret = null)
        {
            if(turret != null && init.Contains<DynamicTurretFacingsInit>())
            {
                Func<int> facing;
                if (init.Get<DynamicTurretFacingsInit, Dictionary<string, Func<int>>>().TryGetValue(turret, out facing))
                    return facing;
            }

            if(turret != null && init.Contains<TurretFacingsInit>())
            {
                int facing;
                if (init.Get<TurretFacingsInit, Dictionary<string, int>>().TryGetValue(turret, out facing))
                    return () => facing;
            }

            if (init.Contains<TurretFacingInit>())
            {
                var facing = init.Get<TurretFacingInit, int>();
                return () => facing;
            }

            if (init.Contains<DynamicFacingInit>())
                return init.Get<DynamicFacingInit, Func<int>>();

            if (init.Contains<FacingInit>())
            {
                var facing = init.Get<FacingInit, int>();
                return () => facing;
            }

            return () => def;
        }

        public Turreted(ActorInitializer init,TurretedInfo info)
        {
            Info = info;
            TurretFacing = TurretFacingFromInit(init, info.InitialFacing, Info.Turret)();
        }


        

        void ITick.Tick(Actor self)
        {
            Tick(self);
        }
        public virtual void Tick(Actor self)
        {
            if(attack != null)
            {

                if (attack.IsAniming)
                    return;

                if (realignTick < Info.RealignDelay)
                    realignTick++;
                else if (Info.RealignDelay > -1)
                    DesiredFacing = null;

                MoveTurret();

            }
            else
            {
                realignTick = 0;
                MoveTurret();
            }
        }


        void MoveTurret()
        {
            var df = DesiredFacing ?? (facing != null ? facing.Facing : TurretFacing);
            TurretFacing = Util.TickFacing(TurretFacing, df, Info.TurnSpeed);
        }

        //Turret offset in world-space.
        public WVec Position(Actor self)
        {
            var bodyOrientation = body.QuantizeOrientation(self, self.Orientation);
            return body.LocalToWorld(Offset.Rotate(bodyOrientation));
        }

        //Orientation in world-space.
        public WRot WorldOrientation(Actor self)
        {
            var world = WRot.FromYaw(WAngle.FromFacing(TurretFacing));

            if (QuantizedFacings == 0)
                return world;

            var facing = body.QuantizeFacing(world.Yaw.Angle / 4, QuantizedFacings);
            return new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(facing));
        }

        void INotifyCreated.Created(Actor self)
        {
            attack = self.TraitsImplementing<AttackTurreted>().SingleOrDefault(at => ((AttackTurretedInfo)at.Info).Turrets.Contains(Info.Turret));
            facing = self.TraitOrDefault<IFacing>();
            body = self.Trait<BodyOrientation>();
        }

        public void ModifyDeathActorInit(Actor self,TypeDictionary init)
        {

        }


        public virtual bool HasAchieveDesiredFacing
        {
            get
            {
                return DesiredFacing == null || TurretFacing == DesiredFacing.Value;
            }
        }


        public bool FaceTarget(Actor self,Target target)
        {
            if (attack == null || attack.IsTraitDisabled || attack.IsTraitPaused)
                return false;

            var pos = self.CenterPosition;
            var targetPos = attack.GetTargetPosition(pos, target);
            var delta = targetPos - pos;
            DesiredFacing = delta.HorizontalLengthSquared != 0 ? delta.Yaw.Facing : TurretFacing;
            MoveTurret();
            return HasAchieveDesiredFacing;
        }
    }

    public class TurretFacingInit : IActorInit<int>
    {
        [FieldFromYamlKey]
        readonly int value = 128;

        public TurretFacingInit() { }

        public TurretFacingInit(int init) { value = init; }

        public int Value(World world) { return value; }
    }


    public class TurretFacingsInit : IActorInit<Dictionary<string, int>>
    {

        [DictionaryFromYamlKey]
        readonly Dictionary<string, int> value = new Dictionary<string, int>();

        public TurretFacingsInit() { }

        public TurretFacingsInit(Dictionary<string,int> init) { value = init; }

        public Dictionary<string,int> Value(World world) { return value; }
    }

    public class DynamicTurretFacingsInit : IActorInit<Dictionary<string, Func<int>>>
    {
        readonly Dictionary<string, Func<int>> value = new Dictionary<string, Func<int>>();

        public DynamicTurretFacingsInit() { }

        public DynamicTurretFacingsInit(Dictionary<string,Func<int>> init) { value = init; }

        public Dictionary<string,Func<int>> Value(World world) { return value; }
    }
}