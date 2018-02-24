using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Graphics;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    public class CarryallInfo : ITraitInfo,Requires<BodyOrientationInfo>,Requires<AircraftInfo>
    {



        public virtual object Create(ActorInitializer init)
        {
            return new Carryall(init.Self,this);
        }
    }
    public class Carryall:INotifyKilled,ISync,ITick,IRender,INotifyActorDisposing
    {

        public enum CarryallState
        {
            Idle,
            Reserved,
            Carrying
        }

        public readonly CarryallInfo Info;

        readonly AircraftInfo aircraftInfo;

        readonly BodyOrientation body;

        readonly IMove move;

        readonly IFacing facing;


        [Sync] public Actor Carryable { get; private set; }

        int cachedFacing;

        IActorPreview[] carryablePreview = null;

        public WVec CarryableOffset { get; private set; }

        public CarryallState State { get; private set; }


        public Carryall(Actor self,CarryallInfo info)
        {
            Info = info;

            Carryable = null;

            State = CarryallState.Idle;

            aircraftInfo = self.Info.TraitInfoOrDefault<AircraftInfo>();
            body = self.Trait<BodyOrientation>();
            move = self.Trait<IMove>();
            facing = self.Trait<IFacing>();
        }

        void ITick.Tick(Actor self)
        {
            if (Carryable != null && Carryable.IsDead)
                DetachCarryable(self);

            if(facing.Facing != cachedFacing)
            {
                self.World.ScreenMap.AddOrUpdate(self);
                cachedFacing = facing.Facing;
            }



        }

        public virtual void DetachCarryable(Actor self)
        {
            UnreserveCarryable(self);
            self.World.ScreenMap.AddOrUpdate(self);

            CarryableOffset = WVec.Zero;
            
        }

        public virtual void UnreserveCarryable(Actor self)
        {
            if (Carryable != null && Carryable.IsInWorld && !Carryable.IsDead)
                Carryable.Trait<Carryable>().UnReserve(Carryable);

            Carryable = null;
            State = CarryallState.Idle;
        }


        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {
            if(State == CarryallState.Carrying)
            {
                if (Carryable.IsInWorld && !Carryable.IsDead)
                    Carryable.Kill(attackInfo.Attacker);
                Carryable = null;
            }

            UnreserveCarryable(self);
        }

        IEnumerable<IRenderable> IRender.Render(Actor self, WorldRenderer wr)
        {
            if(State == CarryallState.Carrying && !Carryable.IsDead)
            {
                if(carryablePreview == null)
                {
                    var carryableInits = new TypeDictionary()
                    {
                        new OwnerInit(Carryable.Owner),
                        new DynamicFacingInit(()=>facing.Facing),
                    };

                    foreach (var api in Carryable.TraitsImplementing<IActorPreviewInitModifier>())
                        api.ModifyActorPreviewInit(Carryable, carryableInits);

                    var init = new ActorPreviewInitializer(Carryable.Info, wr, carryableInits);
                    carryablePreview = Carryable.Info.TraitInfos<IRenderActorPreviewInfo>().SelectMany(rpi => rpi.RenderPreview(init)).ToArray();

                }

                var offset = body.LocalToWorld(CarryableOffset.Rotate(body.QuantizeOrientation(self, self.Orientation)));
                var previewRenderables = carryablePreview.SelectMany(p => p.Render(wr, self.CenterPosition + offset)).OrderBy(WorldRenderer.RenderableScreenZPositionComparisonKey);

                foreach (var r in previewRenderables)
                    yield return r;
            }
        }


        IEnumerable<Rectangle> IRender.ScreenBounds(Actor self, WorldRenderer wr)
        {
            if (carryablePreview == null)
                yield break;

            var pos = self.CenterPosition;
            foreach (var p in carryablePreview)
                foreach (var b in p.ScreenBounds(wr, pos))
                    yield return b;
        }

        void INotifyActorDisposing.Disposing(Actor self)
        {
            if(State == CarryallState.Carrying)
            {
                Carryable.Dispose();
                Carryable = null;

            }
            UnreserveCarryable(self);
        }

    }
}