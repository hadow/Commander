using System;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 飞机的预定着陆地点
    /// </summary>
    [Desc("Reserve landing places for aircraft")]
    class ReservableInfo : TraitInfo<Reservable>{}

    public class Reservable:ITick,INotifyOwnerChanged,INotifyActorDisposing
    {
        Actor reservedFor;
        Aircraft reservedForAircraft;

        void ITick.Tick(Actor self){

            if (reservedFor == null)
                return;

            if(!Target.FromActor(reservedFor).IsValidFor(self))
            {
                reservedForAircraft.UnReserve();
                reservedFor = null;
                reservedForAircraft = null;

            }
            
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self,Player oldOwner,Player newOwner){

            UnReserve();
        }

        void INotifyActorDisposing.Disposing(Actor self){
            UnReserve();
        }

        private void UnReserve(){

            if (reservedForAircraft != null)
                reservedForAircraft.UnReserve();
            
                
        }


        public IDisposable Reserve(Actor self,Actor forActor,Aircraft forAircraft){

            if (reservedForAircraft != null && reservedForAircraft.MayYieldReservation)
                reservedForAircraft.UnReserve();

            reservedFor = forActor;
            reservedForAircraft = forAircraft;

            return new DisposableAction(() => { reservedFor = null; reservedForAircraft = null; }, () => WarGame.RunAfterTick(() =>
            {
                if (WarGame.IsCurrentWorld(self.World))
                    throw new InvalidOperationException("Attempted to finalize  an undisposed DisposableAction.{0} ({1}) reserved {2} ({3}) "
                                                        .F(forActor.Info.Name, forActor.ActorID, self.Info.Name, self.ActorID));


            }));


        }


        public static bool IsReserved(Actor a){

            var res = a.TraitOrDefault<Reservable>();
            return res != null && res.reservedForAircraft != null && !res.reservedForAircraft.MayYieldReservation;
        }
    }
}