using System.Collections.Generic;
using EW.Graphics;
using EW.Traits;
using EW.Primitives;
using EW.Xna.Platforms;
namespace EW
{
    public enum PipType
    {
        Transparent,
        Green,
        Yellow,
        Red,
        Gray,
        Blue,Ammo,
        AmmoEmpty,
    }

    public enum Stance
    {
        None = 0,
        Enemy = 1,      
        Neutral = 2,    //中立国
        Ally = 4,       //同盟国
    }

    public enum TargetModifiers
    {
        None = 0,
        ForceAttack = 1,
        ForceQueue = 2,
        ForceMove = 4,
    }

    public interface IExplodeModifier { bool ShouldExplode(Actor self); }

    /// <summary>
    /// 弹头接口
    /// </summary>
    public interface IWarHead
    {
        int Delay { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="victim">受害方</param>
        /// <param name="firedBy">发射方</param>
        /// <returns></returns>
        bool IsValidAgainst(Actor victim, Actor firedBy);

        bool IsValidAgainst(FrozenActor victim,Actor firedBy);

        void DoImpact(Target target, Actor firedBy, IEnumerable<int> damageModifiers);
    }

    #region Notify Interface

    public interface INotifyIdle { void TickIdle(Actor self); }

    public interface INotifyCreated { void Created(Actor self); }

    public interface INotifyAddToWorld { void AddedToWorld(Actor self); }

    public interface INotifyRemovedFromWorld { void RemovedFromWorld(Actor self); }

    public interface INotifySold { void Selling(Actor self); void Sold(Actor self); }

    #endregion

    #region Render Interface


    public interface IRenderOverlay
    {
        void Render(WorldRenderer wr);
    }
    #endregion

    public interface IAutoSelectionSize { Vector2 SelectionSize(Actor self); }

    /// <summary>
    /// 占领地
    /// </summary>
    public interface IOccupySpace
    {
        WPos CenterPosition { get; }

        CPos TopLeft { get; }

        IEnumerable<Pair<CPos, SubCell>> OccupiedCells();
    }
    public interface IPositionableInfo : ITraitInfoInterface { }
    public interface IPositionable : IOccupySpace
    {
        bool IsLeavingCell(CPos location, SubCell subCell = SubCell.Any);

        bool CanEnterCell(CPos location, Actor ignoreActor = null, bool checkTransientActors = true);

        void SetPosition(Actor self, CPos cPos, SubCell subCell = SubCell.Any);

        void SetPosition(Actor self, WPos wPos);

        void SetVisualPosition(Actor self, WPos wPos);
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

    public class TraitInfo<T>:ITraitInfo where T : new()
    {
        public virtual object Create(ActorInitializer init)
        {
            return new T();
        }
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
        IReadOnlyDictionary<CPos, SubCell> OccupiedCells(ActorInfo info, CPos location, SubCell subCell = SubCell.Any);

        bool SharesCell { get; }
    }

    public interface IFacingInfo : ITraitInfoInterface
    {
        int GetInitialFacing();
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

    public interface ILoadsPalettes
    {
        void LoadPalettes(WorldRenderer wr);
    }

    public interface IPaletteModifier
    {
        void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> b);
    }

    public interface IProvidesAssetBrowserPalettes
    {
        IEnumerable<string> PaletteNames { get; }
    }
}