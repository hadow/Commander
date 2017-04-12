using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Primitives;
using EW.Mobile.Platforms;
namespace EW
{

    public enum TargetModifiers
    {
        None = 0,
        ForceAttack = 1,
        ForceQueue = 2,
        ForceMove = 4,
    }

    #region Notify Interface

    public interface INotifyCreated { void Created(Actor self); }

    public interface INotifyAddToWorld { void AddedToWorld(Actor self); }

    public interface INotifyRemovedFromWorld { void RemovedFromWorld(Actor self); }

    public interface INotifySold { void Selling(Actor self); void Sold(Actor self); }

    #endregion

    public interface IAutoSelectionSize { Vector2 SelectionSize(Actor self); }

    /// <summary>
    /// ’º¡Ïµÿ
    /// </summary>
    public interface IOccupySpace
    {
        WorldPos CenterPosition { get; }

        CellPos TopLeft { get; }

        IEnumerable<Pair<CellPos, SubCell>> OccupiedCells();
    }
    public interface IPositionableInfo : ITraitInfoInterface { }
    public interface IPositionable : IOccupySpace
    {
        bool IsLeavingCell(CellPos location, SubCell subCell = SubCell.Any);

        bool CanEnterCell(CellPos location, Actor ignoreActor = null, bool checkTransientActors = true);

        void SetPosition(Actor self, CellPos cPos, SubCell subCell = SubCell.Any);

        void SetPosition(Actor self, WorldPos wPos);

        void SetVisualPosition(Actor self, WorldPos wPos);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IOrderTargeter
    {
        string OrderID { get; }

        int OrderPriority { get; }

        bool CanTarget(Actor self, Target target, List<Actor> othersAtTarget, ref TargetModifiers modifiers, ref string cursor);

        bool IsQueued { get; }

        bool TargetOverridesSelection(TargetModifiers modifiers);
    }
    public interface IIssueOrder
    {

    }

    public interface IDisabledTrait { bool IsTraitDisabled { get; } }

    /// <summary>
    /// 
    /// </summary>
    public interface IUpgradable
    {
        IEnumerable<string> UpgradeTypes { get; }

        bool AcceptsUpgradeLevel(Actor self, string type, int level);

        void UpgradeLevelChanged(Actor self, string type, int oldLevel, int newLevel);
    }


    public interface ITraitInfoInterface { }

    public interface Requires<T> where T : class, ITraitInfoInterface { }

    public interface ITraitInfo : ITraitInfoInterface
    {
        object Create(ActorInitializer init);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IUpgradableInfo : ITraitInfo { }
    public interface IMove
    {

    }
    public interface IMoveInfo : ITraitInfoInterface { }
    
    public interface IOccupySapceInfo : ITraitInfoInterface
    {
        IReadOnlyDictionary<CellPos, SubCell> OccupiedCells(ActorInfo info, CellPos location, SubCell subCell = SubCell.Any);

        bool SharesCell { get; }
    }

    public interface IFacingInfo : ITraitInfoInterface
    {
        int GetInitialFaceing();
    }

    public interface IFacing
    {
        int TurnSpeed { get; }
        int Facing { get; set; }
    }

    public interface IRulesetLoaded<TInfo> { void RulesetLoaded(Ruleset rules, TInfo info); }

    public interface IRulesetLoaded : IRulesetLoaded<ActorInfo>, ITraitInfoInterface { }


    public interface ITick { void Tick(Actor self); }


    public interface IWorldLoaded { void WorldLoaded(World w,WorldRenderer render); }

    public interface UsesInit<T>:ITraitInfo where T : IActorInit { }
}