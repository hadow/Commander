﻿using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    
    public class CargoInfo : ITraitInfo, Requires<IOccupySpaceInfo>
    {
        /// <summary>
        /// The maximum sum of Passenger.Weight that this actor can support.
        /// </summary>
        public readonly int MaxWeight = 0;

        public readonly string[] InitialUnits = { };

        [GrantedConditionReference]
        public readonly string LoadingCondition = null;

        /// <summary>
        /// The condition to grant to self while passengers are loaded.
        /// Condition can stack with multiple passengers.
        /// </summary>
        [GrantedConditionReference]
        public readonly string LoadedCondition = null;

        /// <summary>
        /// Which direction the passenger will face (relative to the transport) when unloading.
        /// </summary>
        public readonly int PassengerFacing = 128;

        /// <summary>
        /// When this actor dies should all of its passengers be unloaded?
        /// </summary>
        public readonly bool EjectOnDeath = false;

        /// <summary>
        /// Terrain types that this actor is allowed to eject actors onto.Leave empty for all terrain types.
        /// </summary>
        public readonly HashSet<string> UnloadTerrainTypes = new HashSet<string>();
        /// <summary>
        /// Conditions to grant when specified actors are loaded inside the transport.
        /// A dictionary of [actor id]:[condition].
        /// </summary>
        public readonly Dictionary<string, string> PassengerConditions = new Dictionary<string, string>();

        [Desc("Number of pips to display when this actor is selected.")]
        public readonly int PipCount = 0;
        public object Create(ActorInitializer init) { return new Cargo(init, this); }
    }
    public class Cargo:INotifyCreated,INotifyKilled,INotifyAddedToWorld,ITick,INotifyActorDisposing,IPips
    {
        readonly Actor self;
        public readonly CargoInfo Info;
        readonly Stack<Actor> cargo = new Stack<Actor>();
        readonly HashSet<Actor> reserves = new HashSet<Actor>();
        readonly Dictionary<string, Stack<int>> passengerTokens = new Dictionary<string, Stack<int>>();
        Stack<int> loadedTokens = new Stack<int>();
        readonly Lazy<IFacing> facing;
        readonly bool checkTerrainType;
        int totalWeight = 0;
        int reservedWeight = 0;

        bool initialized;

        ConditionManager conditionManager;
        int loadingToken = ConditionManager.InvalidConditionToken;

        CPos currentCell;
        public IEnumerable<CPos> CurrentAdjacentCells { get; private set; }

        public bool Unloading { get; internal set; }
        public IEnumerable<Actor> Passengers{ get { return cargo; }}
        public Cargo(ActorInitializer init,CargoInfo info)
        {

            self = init.Self;
            Info = info;
            Unloading = false;
            checkTerrainType = info.UnloadTerrainTypes.Count > 0;

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

            if(conditionManager!=null && cargo.Any())
            {
                foreach(var c in cargo)
                {
                    string passengerCondition;
                    if (Info.PassengerConditions.TryGetValue(c.Info.Name, out passengerCondition))
                        passengerTokens.GetOrAdd(c.Info.Name).Push(conditionManager.GrantCondition(self, passengerCondition));
                }

                if (!string.IsNullOrEmpty(Info.LoadedCondition))
                    loadedTokens.Push(conditionManager.GrantCondition(self, Info.LoadedCondition));
            }
        }

        void INotifyKilled.Killed(Actor self,AttackInfo info){

            if(Info.EjectOnDeath)
                while((!IsEmpty(self) && CanUnload()))
                {
                    var passenger = Unload(self);
                    var cp = self.CenterPosition;
                    var inAir = self.World.Map.DistanceAboveTerrain(cp).Length != 0;
                    var positionable = passenger.Trait<IPositionable>();
                    positionable.SetPosition(passenger, self.Location);

                    if (!inAir && positionable.CanEnterCell(self.Location, self, false))
                    {
                        self.World.AddFrameEndTask(w => w.Add(passenger));
                        var nbm = passenger.TraitOrDefault<INotifyBlockingMove>();
                        if (nbm != null)
                            nbm.OnNotifyBlockingMove(passenger, passenger);
                    }
                    else
                        passenger.Kill(info.Attacker);
                }

            foreach (var c in cargo)
                c.Kill(info.Attacker);

            cargo.Clear();
        }

        bool CanUnload()
        {
            if (checkTerrainType)
            {
                var terrainType = self.World.Map.GetTerrainInfo(self.Location).Type;

                if (!Info.UnloadTerrainTypes.Contains(terrainType))
                    return false;
            }

            return !IsEmpty(self) && CurrentAdjacentCells != null && CurrentAdjacentCells.Any(c => Passengers.Any(p => p.Trait<IPositionable>().CanEnterCell(c)));
        }


        void INotifyActorDisposing.Disposing(Actor self)
        {
            foreach (var c in cargo)
                c.Dispose();

            cargo.Clear();
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self){

            //Force location update to avoid issues when initial spawn is outside map.
            currentCell = self.Location;
            CurrentAdjacentCells = GetAdjacentCells();
        }

        IEnumerable<CPos> GetAdjacentCells()
        {
            return Util.AdjacentCells(self.World, Target.FromActor(self)).Where(c => self.Location != c);
        }

        void ITick.Tick(Actor self){

            //Notify initial cargo load
            if (!initialized)
            {
                foreach(var c in cargo)
                {
                    c.Trait<Passenger>().Transport = self;

                    foreach (var npe in self.TraitsImplementing<INotifyPassengerEntered>())
                        npe.OnPassengerEntered(self, c);
                }
                initialized = true;
            }
            
            var cell = self.World.Map.CellContaining(self.CenterPosition);
            if(currentCell != cell)
            {
                currentCell = cell;
                CurrentAdjacentCells = GetAdjacentCells();
            }
        }
        /// <summary>
        /// 装载乘客
        /// </summary>
        /// <param name="self"></param>
        /// <param name="a"></param>
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
        /// <summary>
        /// 卸载货物
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public Actor Unload(Actor self)
        {
            var a = cargo.Pop();

            totalWeight -= GetWeight(a);

            SetPassengerFacing(a);

            foreach (var npe in self.TraitsImplementing<INotifyPassengerExited>())
                npe.OnPassengerExited(self, a);

            var p = a.Trait<Passenger>();
            p.Transport = null;

            Stack<int> passengerToken;
            if (passengerTokens.TryGetValue(a.Info.Name, out passengerToken) && passengerToken.Any())
                conditionManager.RevokeCondition(self, passengerToken.Pop());

            if (loadedTokens.Any())
                conditionManager.RevokeCondition(self, loadedTokens.Pop());
            return a;
        }

        void SetPassengerFacing(Actor passenger)
        {
            if (facing.Value == null)
                return;

            var passengerFacing = passenger.TraitOrDefault<IFacing>();
            if (passengerFacing != null)
                passengerFacing.Facing = facing.Value.Facing + Info.PassengerFacing;

            foreach (var t in passenger.TraitsImplementing<Turreted>())
                t.TurretFacing = facing.Value.Facing + Info.PassengerFacing;


        }

        public bool HasSpace(int weight) { return totalWeight + reservedWeight + weight <= Info.MaxWeight; }


        public bool IsEmpty(Actor self) { return cargo.Count == 0; }


        public Actor Peek(Actor self)
        {
            return cargo.Peek();
        }


        public IEnumerable<PipType> GetPips(Actor self)
        {
            var numPips = Info.PipCount;

            for (var i = 0; i < numPips; i++)
                yield return GetPipAt(i);
                
        }


        PipType GetPipAt(int i)
        {
            var n = i * Info.MaxWeight / Info.PipCount;

            foreach(var c in cargo)
            {
                var pi = c.Info.TraitInfo<PassengerInfo>();
                if (n < pi.Weight)
                    return pi.PipType;
                else
                    n -= pi.Weight;

            }

            return PipType.Transparent;
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