using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Primitives;
using EW.Activities;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{

    public class AircraftInfo : ITraitInfo,IPositionableInfo,IFacingInfo,IMoveInfo,UsesInit<LocationInit>,UsesInit<FacingInit>
    {
        //巡航高度
        public readonly WDist CruiseAltitude = new WDist(1280);
        public readonly WDist IdealSeparation = new WDist(1706);
        //是否可以击退飞机
        [Desc("Whether the aircraft can be repulsed.")]
        public readonly bool Repulsable = true;

        [Desc("The speed at which the aircraft is repulsed from other aircraft. Specify -1 for normal movement speed.")]
        public readonly int RepulsionSpeed = -1;

        public readonly int InitialFacing = 0;

        public readonly int TurnSpeed = 255;

        public readonly int Speed = 1;

        [ActorReference]
        public readonly HashSet<string> RepairBuildings = new HashSet<string>();

        [ActorReference]
        public readonly HashSet<string> RearmBuildings = new HashSet<string>();
        /// <summary>
        /// The landable terrain types.
        /// 可以着陆的地形列表
        /// </summary>
        public readonly HashSet<string> LandableTerrainTypes = new HashSet<string>();

        [Desc("Altitude at which aircraft considers itself landed.")]
        public readonly WDist LandAltitude = WDist.Zero;

        public readonly bool MoveIntoShroud = true;

        [Desc("Minimum altitude where this aircraft is considered airborne.")]
        public readonly int MinAirborneAltitude = 1;

        [Desc("The condition to grant to self while airborne.")]
        [GrantedConditionReference]
        public readonly string AirborneCondition = null;

        [Desc("The condition to grant to self while at cruise altitude")]
        [GrantedConditionReference]
        public readonly string CruisingCondition = null;

        [Desc("Does this actor automatically take off after creation ?")]
        public readonly bool TakeOffOnCreation = true;

        [Desc("Does this actor need to turn before landing?")]
        public readonly bool TurnToLand = false;
        
        [Desc("Does this actor cancel its previous activity after resupplying?")]
        public readonly bool AbortOnResupply = true;

        [Desc("Sound to play when the actor is landing.")]
        public readonly string LandingSound = null;

        public readonly string TakeoffSound = null;

        [Desc("How fast this actor ascends or descends when using vertical take off /landing.")]
        public readonly WDist AltitudeVelocity = new WDist(43);

        [Desc("Will this actor try to land after it has no more commands?")]
        public readonly bool LandWhenIdle = true;

        [Desc("The distance of the resupply base that the aircraft will wait for its turn.")]
        public readonly WDist WaitDistanceFromResupplyBase = new WDist(3072);


        [Desc("Can the actor hover in place mid-air ?  If not,then the actor will have to remain in motion (circle around).")]
        public readonly bool CanHover = false;

        [Desc("Does the actor land and take off vertically?")]
        public readonly bool VTOL = true;

        [Desc("How fast this actor ascends or descends when using horizontal take off/landing.")]
        public readonly WAngle MaximumPitch = WAngle.FromDegrees(10);
        [VoiceReference]
        public readonly string Voice = "Action";


        public object Create(ActorInitializer init)
        {
            return new Aircraft(init,this);
        }


        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,bool checkTransientActors = true){

            if (!world.Map.Contains(cell))
                return false;

            var type = world.Map.GetTerrainInfo(cell).Type;

            if (!LandableTerrainTypes.Contains(type))
                return false;

            if (world.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(cell) != null)
                return false;

            if (!checkTransientActors)
                return true;

            return !world.ActorMap.GetActorsAt(cell).Any(x => x != ignoreActor);
        }

        bool IOccupySpaceInfo.SharesCell{ get { return false; }}

        public IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any) { return new ReadOnlyDictionary<CPos, SubCell>(); }


        public int GetInitialFacing() { return InitialFacing; }
    }

    public class Aircraft:ITick,ISync,IFacing,IPositionable,IMove,IDeathActorInitModifier,INotifyCreated,INotifyAddedToWorld,INotifyRemovedFromWorld,INotifyActorDisposing
    {

        static readonly Pair<CPos, SubCell>[] NoCells = { };

        public readonly AircraftInfo Info;

        readonly Actor self;

        ConditionManager conditionManager;
        IDisposable reservation;            //保留

        bool airborne;//空运
        bool cruising;//巡航
        bool firstTick = true;


        bool isMoving;
        bool isMovingVertically;
        WPos cachedPosition;
        bool? landNow;

        IEnumerable<int> speedModifiers;

        int airborneToken = ConditionManager.InvalidConditionToken;
        int cruisingToken = ConditionManager.InvalidConditionToken;

        public Actor ReservedActor { get; private set; }
        public bool MayYieldReservation { get; private set;}
        public bool ForceLanding { get; private set; }
        [Sync]
        public int Facing { get; set; }

        [Sync]
        public WPos CenterPosition { get; private set; }


        public int TurnSpeed { get { return Info.TurnSpeed; } }

        public CPos TopLeft { get { return self.World.Map.CellContaining(CenterPosition); } }

        public Pair<CPos,SubCell>[] OccupiedCells() { return NoCells; }


        public Aircraft(ActorInitializer init,AircraftInfo info){

            Info = info;
            self = init.Self;

            if (init.Contains<LocationInit>())
                SetPosition(self, init.Get<LocationInit, CPos>());

            if (init.Contains<CenterPositionInit>())
                SetPosition(self, init.Get<CenterPositionInit, WPos>());

            Facing = init.Contains<FacingInit>() ? init.Get<FacingInit, int>() : Info.InitialFacing;

        }
        void INotifyCreated.Created(Actor self)
        {
            Created(self);
        }

        protected virtual void Created(Actor self){
            conditionManager = self.TraitOrDefault<ConditionManager>();
            speedModifiers = self.TraitsImplementing<ISpeedModifier>().ToArray().Select(sm => sm.GetSpeedModifier());
            cachedPosition = self.CenterPosition;
        }



        void ITick.Tick(Actor self)
        {
            Tick(self);
        }

        protected virtual void Tick(Actor self){
            if(firstTick){
                firstTick = false;

                if (self.Info.HasTraitInfo<FallsToEarthInfo>())
                    return;

                ReserveSpawnBuilding();

                var host = GetActorBelow();
                if (host == null)
                    return;

                if (Info.TakeOffOnCreation)
                    self.QueueActivity(new TakeOff(self));

            }

            // Add land activity if LandOnCondidion resolves to true and the actor can land at the current location.

            if(landNow.HasValue && landNow.Value && airborne && CanLand(self.Location)&&
               !(self.CurrentActivity is Turn || self.CurrentActivity is HeliLand )){
                self.CancelActivity();

                if (Info.TurnToLand)
                    self.QueueActivity(new Turn(self, Info.InitialFacing));

                self.QueueActivity(new HeliLand(self,true));


                ForceLanding = true;

            }

            if(landNow.HasValue && !landNow.Value && !cruising && !(self.CurrentActivity is TakeOff)){

                ForceLanding = false;

                if(!Info.LandWhenIdle)
                {
                    self.CancelActivity();
                    self.QueueActivity(new TakeOff(self));
                }

            }

            var oldCachedPosition = cachedPosition;
            cachedPosition = self.CenterPosition;
            isMoving = (oldCachedPosition - cachedPosition).HorizontalLengthSquared != 0;
            isMovingVertically = (oldCachedPosition - cachedPosition).VerticalLengthSquared != 0;

            Repulse();


        }

        /// <summary>
        /// Repulse this instance.
        /// </summary>
        public void Repulse(){

            var repulsionForce = GetRepulsionForce();
            if (repulsionForce.HorizontalLengthSquared == 0)
                return;

            var speed = Info.RepulsionSpeed != -1 ? Info.RepulsionSpeed : MovementSpeed;
            SetPosition(self,CenterPosition + FlyStep(speed,repulsionForce.Yaw.Facing));

        }

        public virtual WVec GetRepulsionForce(){

            if (!Info.Repulsable)
                return WVec.Zero;

            if(reservation != null){

                var distanceFromReservationActor = (ReservedActor.CenterPosition - self.CenterPosition).HorizontalLength;
                if (distanceFromReservationActor < Info.WaitDistanceFromResupplyBase.Length)
                    return WVec.Zero;
                
            }


            var altitude = self.World.Map.DistanceAboveTerrain(CenterPosition).Length;
            if (altitude != Info.CruiseAltitude.Length)
                return WVec.Zero;

            var repulsionForce = WVec.Zero;

            foreach(var actor in self.World.FindActorsInCircle(self.CenterPosition,Info.IdealSeparation)){
                if (actor.IsDead)
                    continue;

                var ai = actor.Info.TraitInfoOrDefault<AircraftInfo>();

                if (ai == null || !ai.Repulsable || ai.CruiseAltitude != Info.CruiseAltitude)
                    continue;

                repulsionForce += GetRepulsionForce(actor);
            }

            if(!self.World.Map.Contains(self.Location))
            {
                var center = WPos.Lerp(self.World.Map.ProjectedTopLeft, self.World.Map.ProjectedBottomRight, 1, 2);
                repulsionForce += new WVec(1024, 0, 0).Rotate(WRot.FromYaw((self.CenterPosition - center).Yaw));
            }

            if (Info.CanHover)
                return repulsionForce;

            var currentDir = FlyStep(Facing);

            var length = currentDir.HorizontalLength * repulsionForce.HorizontalLength;
            if (length == 0)
                return WVec.Zero;

            var dot = WVec.Dot(currentDir, repulsionForce) / length;

            //avoid stalling the plane
            return dot >= 0 ? repulsionForce : WVec.Zero;



        }


        public WVec GetRepulsionForce(Actor other){

            if (self == other || other.CenterPosition.Z < self.CenterPosition.Z)
                return WVec.Zero;

            var d = self.CenterPosition - other.CenterPosition;
            var distSq = d.HorizontalLengthSquared;

            if (distSq > Info.IdealSeparation.LengthSquared)
                return WVec.Zero;

            if(distSq<1)
            {

                var yaw = self.World.SharedRandom.Next(0, 1023);
                var rot = new WRot(WAngle.Zero, WAngle.Zero, new WAngle(yaw));

                return new WVec(1024, 0, 0).Rotate(rot);

            }

            return (d * 1024 * 8) / (int)distSq;

        }




        protected void ReserveSpawnBuilding(){

            var spawner = GetActorBelow();
            if (spawner == null)
                return;
            MakeReservation(spawner);
        }

        public void MakeReservation(Actor target){

            UnReserve();
            var reservable = target.TraitOrDefault<Reservable>();
            if(reservable != null)
            {
                reservation = reservable.Reserve(target, self, this);
                ReservedActor = target;
            }
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            AddedToWorld(self);
        }

        protected virtual void AddedToWorld(Actor self)
        {
            self.World.AddToMaps(self, this);

            var altitude = self.World.Map.DistanceAboveTerrain(self.CenterPosition);
            if(altitude.Length >= Info.MinAirborneAltitude){
                OnAirborneAltitudeReached();
            }

            if (altitude == Info.CruiseAltitude)
                OnCruisingAltitudeReached();
            

        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            RemovedFromWorld(self);
        }

        protected virtual void RemovedFromWorld(Actor self){

            UnReserve();

            self.World.RemoveFromMaps(self,this);

            OnCruisingAltitudeLeft();
            OnAirborneAltitudeLeft();


        }

        void INotifyActorDisposing.Disposing(Actor self)
        {
            UnReserve();
        }

        void IDeathActorInitModifier.ModifyDeathActorInit(Actor self, Primitives.TypeDictionary inits)
        {
            inits.Add(new FacingInit(Facing));
        }


        public int MovementSpeed{
            get { return Util.ApplyPercentageModifiers(Info.Speed, speedModifiers); }
        }

        public bool CanLand(CPos cell){
            if (!self.World.Map.Contains(cell))
                return false;

            if (self.World.ActorMap.AnyActorsAt(cell))
                return false;

            var type = self.World.Map.GetTerrainInfo(cell).Type;

            return Info.LandableTerrainTypes.Contains(type);
        }


        public WVec FlyStep(int facing){

            return FlyStep(MovementSpeed, facing);
        }

        public WVec FlyStep(int speed,int facing){

            var dir = new WVec(0, -1024, 0).Rotate(WRot.FromFacing(facing));
            return speed * dir / 1024;

        }

        public Actor GetActorBelow(){

            if (self.World.Map.DistanceAboveTerrain(CenterPosition).Length != 0)
                return null; //not on the ground.

            return self.World.ActorMap.GetActorsAt(self.Location)
                       .FirstOrDefault(a => a.Info.HasTraitInfo<ReservableInfo>());
            
        }

        #region Implement IPositionable

        public bool IsLeavingCell(CPos location,SubCell subCell = SubCell.Any) { return false; }

        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true) { return true; }

        public SubCell GetValidSubCell(SubCell preferred) { return SubCell.Invalid; }

        public SubCell GetAvailableSubCell(CPos a,SubCell preferredSubCell = SubCell.Any,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            //Does not use any Subcell
            return SubCell.Invalid;
        }


        public void SetVisualPosition(Actor self,WPos pos) { SetPosition(self, pos); }

        public void SetPosition(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {
            SetPosition(self,self.World.Map.CenterOfCell(cell) + new WVec(0,0,CenterPosition.Z));
        }


        public void SetPosition(Actor self,WPos pos)
        {
            CenterPosition = pos;

            if (!self.IsInWorld)
                return;

            self.World.UpdateMaps(self,this);

            var altitude = self.World.Map.DistanceAboveTerrain(CenterPosition);

            var isAirborne = altitude.Length >= Info.MinAirborneAltitude;

            if(isAirborne && !airborne){
                OnAirborneAltitudeReached();
            }
            else if(!isAirborne && airborne){
                OnAirborneAltitudeLeft();
            }

            var isCruising = altitude == Info.CruiseAltitude;
            if(isCruising  && !cruising)
            {
                OnCruisingAltitudeReached();
            }
            else if(!isCruising && cruising){
                OnCruisingAltitudeLeft();
            }

        }



        #endregion

        public void UnReserve(){
            if (reservation == null)
                return;

            reservation.Dispose();
            reservation = null;
            ReservedActor = null;

            if (self.World.Map.DistanceAboveTerrain(CenterPosition).Length <= Info.LandAltitude.Length)
                self.QueueActivity(new TakeOff(self));
            


        }


        void OnCruisingAltitudeReached()
        {
            if (cruising)
                return;

            cruising = true;
            if (conditionManager != null && !string.IsNullOrEmpty(Info.CruisingCondition) && cruisingToken == ConditionManager.InvalidConditionToken)
                cruisingToken = conditionManager.GrantCondition(self, Info.CruisingCondition);
            
        }

        void OnCruisingAltitudeLeft(){

            if (!cruising)
                return;

            cruising = false;

            if (conditionManager != null && cruisingToken != ConditionManager.InvalidConditionToken)
                cruisingToken = conditionManager.RevokeCondition(self, cruisingToken);
        }

        void OnAirborneAltitudeReached(){
            if (airborne)
                return;

            airborne = true;

            if (conditionManager != null && !string.IsNullOrEmpty(Info.AirborneCondition) && airborneToken == ConditionManager.InvalidConditionToken)
                airborneToken = conditionManager.GrantCondition(self, Info.AirborneCondition);
            
        }

        void OnAirborneAltitudeLeft(){

            if (!airborne)
                return;

            airborne = false;

            if (conditionManager != null && airborneToken != ConditionManager.InvalidConditionToken)
                airborneToken = conditionManager.RevokeCondition(self, airborneToken);
        }

        #region Implement IMove

        public Activity MoveTo(CPos cell,int nearEnough)
        {
            if (!Info.CanHover)
                return new FlyAndContinueWithCirclesWhenIdle(self, Target.FromCell(self.World, cell));
            return new HeliFly(self, Target.FromCell(self.World, cell));
        }

        public Activity MoveTo(CPos cell,Actor ignoreActor)
        {
            if (!Info.CanHover)
                return new FlyAndContinueWithCirclesWhenIdle(self, Target.FromCell(self.World, cell));

            return new HeliFly(self, Target.FromCell(self.World, cell));
        }

        public Activity MoveWithinRange(Target target,WDist range)
        {
            if (!Info.CanHover)
                return new FlyAndContinueWithCirclesWhenIdle(self, target, WDist.Zero, range);

            return new HeliFly(self, target, WDist.Zero, range);
        }

        public Activity MoveWithinRange(Target target,WDist minRange,WDist maxRange)
        {
            if (!Info.CanHover)
                return new FlyAndContinueWithCirclesWhenIdle(self, target, minRange, maxRange);

            return new HeliFly(self, target, minRange, maxRange);
        }


        public Activity MoveFollow(Actor self,Target target,WDist minRange,WDist maxRange)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoWorld(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {
            throw new NotImplementedException();
        }

        public Activity MoveToTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity VisualMove(Actor self,WPos fromPos,WPos toPos)
        {
            throw new NotImplementedException();
        }


        public CPos NearestMoveableCell(CPos cell) { return cell; }

        public bool IsMoving { get { return isMoving; }set { } }

        public bool CanEnterTargetNow(Actor self,Target target)
        {

            if (target.Positions.Any(p => self.World.ActorMap.GetActorsAt(self.World.Map.CellContaining(p)).Any(a => a != self && a != target.Actor)))
                return false;

            MakeReservation(target.Actor);
            return true;
        }
        #endregion
    }
}