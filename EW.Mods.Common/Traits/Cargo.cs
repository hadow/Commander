using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    public interface INotifyPassengerEntered { void OnPassengerEntered(Actor self, Actor passenger); }

    public interface INotifyPassengerExited { void OnPassengerExited(Actor self, Actor passenger); }
    public class CargoInfo : ITraitInfo, Requires<IOccupySpaceInfo>
    {

        public readonly string[] InitialUnits = { };

        public readonly string[] LoadingUpgrades = { };

        public object Create(ActorInitializer init) { return new Cargo(init, this); }
    }
    public class Cargo:INotifyCreated,INotifyKilled,INotifyAddedToWorld,ITick
    {
        readonly Actor self;
        public readonly CargoInfo Info;
        readonly Stack<Actor> cargo = new Stack<Actor>();
        readonly HashSet<Actor> reserves = new HashSet<Actor>();

        readonly Lazy<IFacing> facing;
        int totalWeight = 0;
        int reservedWeight = 0;

        bool initialized;

        ConditionManager conditionManager;
        int loadingToken = ConditionManager.InvalidConditionToken;

        CPos currentCell;
        public IEnumerable<CPos> CurrentAdjacentCells { get; private set; }

        public IEnumerable<Actor> Passengers{ get { return cargo; }}
        public Cargo(ActorInitializer init,CargoInfo info)
        {

            self = init.Self;
            Info = info;

            if(init.Contains<RuntimeCargoInit>()){
                cargo = new Stack<Actor>(init.Get<RuntimeCargoInit, Actor[]>());
            }
            else if(init.Contains<CargoInit>()){

                foreach(var u in init.Get<CargoInit,string[]>())
                {
                    var unit = self.World.CreateActor(false, u.ToLowerInvariant(), new TypeDictionary() { new OwnerInit(self.Owner) });

                    cargo.Push(unit);
                }

            }
            else{

                foreach(var u in info.InitialUnits){
                    var unit = self.World.CreateActor(false, u.ToLowerInvariant(), new TypeDictionary() { new OwnerInit(self.Owner) });

                    cargo.Push(unit);
                }

            }

            totalWeight = cargo.Sum(c => GetWeight(c));

            facing = Exts.Lazy(self.TraitOrDefault<IFacing>);
        }

        static int GetWeight(Actor a){
            return a.Info.TraitInfo<PassengerInfo>().Weight;
        }


        void INotifyCreated.Created(Actor self){


            conditionManager = self.TraitOrDefault<ConditionManager>();

        }

        void INotifyKilled.Killed(Actor self,AttackInfo info){


        }

        void INotifyAddedToWorld.AddedToWorld(Actor self){

            currentCell = self.Location;

        }

        void ITick.Tick(Actor self){


            var cell = self.World.Map.CellContaining(self.CenterPosition);
            if(currentCell != cell)
            {
                currentCell = cell;

            }
        }

        public void Load(Actor self,Actor a)
        {
            cargo.Push(a);
            var w = GetWeight(a);
            totalWeight += w;
            if (reserves.Contains(a))
            {
                reservedWeight -= w;
                reserves.Remove(a);

                if (loadingToken != ConditionManager.InvalidConditionToken)
                    loadingToken = conditionManager.RevokeCondition(self, loadingToken);
            }

            if (initialized)
                foreach (var npe in self.TraitsImplementing<INotifyPassengerEntered>())
                    npe.OnPassengerEntered(self, a);

            var p = a.Trait<Passenger>();
            p.Transport = self;

        }

        public Actor Unload(Actor self)
        {
            var a = cargo.Pop();
            return a;
        }

    }


    public class RuntimeCargoInit:IActorInit<Actor[]>{

        [FieldFromYamlKey]
        readonly Actor[] value = { };

        public RuntimeCargoInit(){}

        public RuntimeCargoInit(Actor[] init) { value = init; }

        public Actor[] Value(World world){
            return value;
        }
    }

    public class CargoInit:IActorInit<string[]>{

        [FieldFromYamlKey]
        readonly string[] value = { };

        public CargoInit(){}

        public CargoInit(string[] init){
            value = init;
        }

        public string[] Value(World world){
            return value;
        }

    }

}