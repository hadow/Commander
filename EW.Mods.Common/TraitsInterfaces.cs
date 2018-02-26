using System;
using System.Collections.Generic;
using EW.Mods.Common.Graphics;
using System.Drawing;
using EW.Traits;
using EW.Primitives;
using EW.Graphics;
using EW.Mods.Common.Traits.Render;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{
    public enum VisibilityType { Footprint, CenterPosition, GroundPosition }


    public interface IPlaceBuildingDecorationInfo : ITraitInfo
    {
        IEnumerable<IRenderable> Render(WorldRenderer wr, World w, ActorInfo ai, WPos centerPosition);
    }

    #region Notify

    public interface INotifyObjectivesUpdated
    {
        void OnPlayerWon(Player winner);

        void OnPlayerLost(Player loser);

        void OnObjectiveAdded(Player player, int objectiveID);

        void OnObjectiveCompleted(Player player, int objectiveID);

        void OnObjectiveFailed(Player player, int objectiveID);
    }


    [RequireExplicitImplementation]
    public interface INotifyPowerLevelChanged { void PowerLevelChanged(Actor self); }

    public interface INotifyDocking { void Docked(Actor self, Actor harvester); void Undocked(Actor self, Actor harvester); }

    public interface INotifyPassengerEntered { void OnPassengerEntered(Actor self, Actor passenger); }

    public interface INotifyPassengerExited { void OnPassengerExited(Actor self, Actor passenger); }

    public interface INotifyHarvesterAction
    {
        //void MovingToResources(Actor self, CPos targetCell, Activity next);

        //void MovingToRefinery(Actor self, Actor refineryActor, Activity next);

        //void MovementCancelled(Actor self);

        void Harvested(Actor self, ResourceType resource);

        void Docked();

        void Undocked();
    }


    public interface INotifyBlockingMove { void OnNotifyBlockingMove(Actor self, Actor blocking); }

    public interface INotifyDeployComplete
    {
        void FinishedDeploy(Actor self);

        void FinishedUndeploy(Actor self);
    }


    public interface INotifyDeployTriggered
    {
        void Deploy(Actor self, bool skipMakeAnim);

        void Undeploy(Actor self, bool skipMakeAnim);
    }
    public interface INotifyProduction { void UnitProduced(Actor self, Actor other, CPos exit); }

    public interface INotifyOtherProduction { void UnitProducedByOther(Actor self, Actor producer, Actor produced, string productionType); }

    [RequireExplicitImplementation]
    public interface INotifyBuildComplete { void BuildingComplete(Actor self); }


    public interface INotifyBurstComplete { void FiredBurst(Actor self, Target target, Armament a); }
    [RequireExplicitImplementation]
    public interface INotifyAttack
    {

        void PreparingAttack(Actor self, Target target, Armament a, Barrel barrel);
        void Attacking(Actor self, Target target, Armament a, Barrel barrel);
    }

    public interface INotifyDamage{
        void Damaged(Actor self, AttackInfo attackInfo);
    }

    [RequireExplicitImplementation]
    public interface INotifyDamageStateChanged { void DamageStateChanged(Actor self, AttackInfo attackInfo); }

    public interface INotifyKilled{
        void Killed(Actor self, AttackInfo attackInfo);
    }

    public interface INotifyAppliedDamage{
        void AppliedDamage(Actor self, Actor damaged, AttackInfo attackInfo);
    }

    [RequireExplicitImplementation]
    public interface INotifyUnload
    {
        void Unloading(Actor self);
    }


    #endregion
    [RequireExplicitImplementation]
    public interface IBlocksProjectiles
    {

        WDist BlockingHeight { get; }
    }

    [RequireExplicitImplementation]
    public interface IBlocksProjectilesInfo : ITraitInfoInterface { }

    interface IWallConnector
    {
        bool AdjacentWallCanConnect(Actor self, CPos wallLocation, string wallType, out CVec facing);

        void SetDirty();
    }

    [RequireExplicitImplementation]
    public interface ICustomMovementLayer
    {
        byte Index { get; }

        bool InteractsWithDefaultLayer { get; }

        bool EnabledForActor(ActorInfo ai, MobileInfo mi);

        int EntryMovementCost(ActorInfo ai, MobileInfo mi, CPos cell);

        int ExitMovementCost(ActorInfo ai, MobileInfo mi, CPos cell);

        byte GetTerrainIndex(CPos cell);

        WPos CenterOfCell(CPos cell);
    }

    [RequireExplicitImplementation]
    public interface IRenderInfantrySequenceModifier
    {
        bool IsModifyingSequence { get; }

        string SequencePrefix { get; }
    }

    [RequireExplicitImplementation]
    public interface ICrushable
    {
        bool CrushableBy(Actor self, Actor crusher, HashSet<string> crushClasses);
    }
    public interface IObservesVariables
    {
        IEnumerable<VariableObserver> GetVariableObservers();
    }
    [RequireExplicitImplementation]
    public interface IObservesVariablesInfo : ITraitInfo
    {

    }

    public interface IRenderActorPreviewInfo : ITraitInfo
    {
        IEnumerable<IActorPreview> RenderPreview(ActorPreviewInitializer init);
    }

    public interface IRenderActorPreviewVoxelsInfo : ITraitInfo
    {
        IEnumerable<ModelAnimation> RenderPreviewVoxels(ActorPreviewInitializer init, RenderVoxelsInfo rv, string image, Func<WRot> orientation, int facings, PaletteReference p);
    }



    public interface IConditionConsumerInfo : ITraitInfo { }

    




    public interface IAcceptResourcesInfo : ITraitInfo { }

    public interface IAcceptResources
    {
        void OnDock(Actor harv, DeliverResources dockOrder);

        void GiveResource(int amount);

        bool CanGiveResource(int amount);

        CVec DeliveryOffset { get; }

        bool AllowDocking { get; }
    }

    public interface IQuantizeBodyOrientationInfo : ITraitInfo
    {
        int QuantizedBodyFacings(ActorInfo ai, SequenceProvider sequenceProvider, string race);
    }


    public delegate void VariableObserverNotifier(Actor self, IReadOnlyDictionary<string, int> variables);

    /// <summary>
    /// 观察变量
    /// </summary>
    public struct VariableObserver
    {
        public VariableObserverNotifier Notifier;

        public IEnumerable<string> Variables;

        public VariableObserver(VariableObserverNotifier notifier, IEnumerable<string> variables)
        {
            Notifier = notifier;
            Variables = variables;
        }
    }

    public interface IPreventsAutoTarget
    {
        bool PreventsAutoTarget(Actor self, Actor attacker);
    }


    #region Modifier
    [RequireExplicitImplementation]
    public interface IRangeModifier { int GetRangeModifier(); }

    [RequireExplicitImplementation]
    public interface IRangeModifierInfo : ITraitInfoInterface { int GetRangeModifierDefault(); }

    [RequireExplicitImplementation]
    public interface IPowerModifier { int GetPowerModifier(); }


    public interface IDeathActorInitModifier
    {
        void ModifyDeathActorInit(Actor self, TypeDictionary inits);
    }

    public interface IHuskModifier { string HuskActor(Actor self); }


    public interface IActorPreviewInitModifier
    {
        void ModifyActorPreviewInit(Actor self, TypeDictionary inits);
    }

    public interface IRadarColorModifier { Color RadarColorOverride(Actor self, Color color); }

    #endregion


    public interface IRadarSignature
    {
        void PopulateRadarSignatureCells(Actor self, List<Pair<CPos, Color>> destinationBuffer);
    }


    public interface ITechTreeElement
    {
        void PrerequisitesAvailable(string key);

        void PrerequisitesUnavailable(string key);

        void PrerequisitesItemHidden(string key);

        void PrerequisitesItemVisible(string key);
    }


    public interface ITechTreePrerequisiteInfo : ITraitInfo { }

    public interface ITechTreePrerequisite
    {
        IEnumerable<string> ProvidesPrerequisites { get; }
    }

    public enum ActorPreviewType { PlaceBuilding,ColorPicker,MapEditorSidebar}
    public interface IActorPreviewInitInfo : ITraitInfo
    {
        IEnumerable<object> ActorPreviewInits(ActorInfo ai, ActorPreviewType type);
    }

}
