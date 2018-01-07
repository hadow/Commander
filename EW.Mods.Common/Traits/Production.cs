using System;
using System.Drawing;
using EW.Traits;
using EW.Primitives;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{

    public class ProductionInfo : PausableConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new Production(init, this);
        }
    }


    public class Production:PausableConditionalTrait<ProductionInfo>,INotifyCreated
    {

        readonly Lazy<RallyPoint> rp;
        public string Faction { get; private set; }

        Building building;
        public Production(ActorInitializer init,ProductionInfo info):base(info)
        {

            rp = Exts.Lazy(() => init.Self.IsDead ? null : init.Self.TraitOrDefault<RallyPoint>());

            Faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : init.Self.Owner.Faction.InternalName;

        }

        public virtual void DoProduction(Actor self,ActorInfo producee,ExitInfo exitinfo,string productionType,TypeDictionary inits){


            var exit = CPos.Zero;
            var exitLocation = CPos.Zero;
            var target = Target.Invalid;

            var td = new TypeDictionary();
            foreach (var init in inits)
                td.Add(init);

            if(self.OccupiesSpace!=null){

                exit = self.Location + exitinfo.ExitCell;

                var spawn = self.CenterPosition + exitinfo.SpawnOffset;
                var to = self.World.Map.CenterOfCell(exit);

                var initialFacing = exitinfo.Facing;

                if(exitinfo.Facing<0){
                    
                }




                td.Add(new LocationInit(exit));
                td.Add(new CenterPositionInit(spawn));
                td.Add(new FacingInit(initialFacing));
            }


            self.World.AddFrameEndTask(w=>{


                var newUnit = self.World.CreateActor(producee.Name, td);

                var move = newUnit.TraitOrDefault<IMove>();

                if(move !=null){

                    if(exitinfo.MoveIntoWorld){

                        if (exitinfo.ExitDelay > 0)
                            newUnit.QueueActivity(new Wait(exitinfo.ExitDelay, false));

                        newUnit.QueueActivity(move.MoveIntoWorld(newUnit,exit));
                        newUnit.QueueActivity(new AttackMoveActivity(newUnit,move.MoveTo(exitLocation,1)));
                    }
                }

                newUnit.SetTargetLine(target,rp.Value != null ?Color.Red:Color.Green,false);

                if (!self.IsDead)
                    foreach (var t in self.TraitsImplementing<INotifyProduction>())
                        t.UnitProduced(self, newUnit, exit);

                var notifyOthers = self.World.ActorsWithTrait<INotifyOtherProduction>();

                foreach(var notify in notifyOthers){
                    notify.Trait.UnitProducedByOther(notify.Actor,self,newUnit,productionType);

                }

                foreach (var t in newUnit.TraitsImplementing<INotifyBuildComplete>())
                    t.BuildingComplete(newUnit);

            });



        }


        void INotifyCreated.Created(Actor self)
        {
            building = self.TraitOrDefault<Building>();

        }

        protected virtual ExitInfo SelectExit(Actor self,ActorInfo producee,string productionType,Func<ExitInfo,bool> p){

            return self.RandomExitOrDefault(productionType, p);

        }

        protected ExitInfo SelectExit(Actor self,ActorInfo producee,string productionType){

            return SelectExit(self, producee, productionType, e => CanUseExit(self, producee, e));
        }


        public virtual bool Produce(Actor self,ActorInfo producee,string productionType,TypeDictionary inits)
        {


            if (IsTraitDisabled || IsTraitPaused)
                return false;

            var exit = SelectExit(self, producee, productionType);

            if(exit != null || self.OccupiesSpace != null)
            {
                DoProduction(self,producee,exit,productionType,inits);
                return true;
            }
            return false;
        }


        static bool CanUseExit(Actor self,ActorInfo producee,ExitInfo s){

            var mobileInfo = producee.TraitInfoOrDefault<MobileInfo>();

            return mobileInfo == null || mobileInfo.CanEnterCell(self.World, self, self.Location + s.ExitCell, self);
        }
    }
}