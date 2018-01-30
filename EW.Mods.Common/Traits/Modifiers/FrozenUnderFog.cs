using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Primitives;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{


    public class FrozenUnderFogInfo : ITraitInfo, Requires<BuildingInfo>
    {

        public object Create(ActorInitializer init) { return new FrozenUnderFog(init,this); }
    }

    public class FrozenUnderFog:IRenderModifier,ITickRender,IDefaultVisibility,ITick,ISync,INotifyCreated
    {
        class FrozenState
        {
            public readonly FrozenActor FrozenActor;
            public bool IsVisible;
            public FrozenState(FrozenActor frozenActor) { FrozenActor = frozenActor; }
        }

        [Sync]
        public int VisibilityHash;

        readonly FrozenUnderFogInfo info;

        readonly bool startsRevealed;

        readonly PPos[] footprint;

        PlayerDictionary<FrozenState> frozenStates;

        bool isRendering;

        public FrozenUnderFog(ActorInitializer init,FrozenUnderFogInfo info)
        {
            this.info = info;

            var map = init.World.Map;

            //Explore map-placed actors if the "Explore Map" option is enabled;
            var shroudInfo = init.World.Map.Rules.Actors["player"].TraitInfo<ShroudInfo>();
            var exploredMap = init.World.LobbyInfo.GlobalSettings.OptionOrDefault("explored", shroudInfo.ExploredMapCheckboxEnabled);
            startsRevealed = exploredMap && init.Contains<SpawnedByMapInit>() && !init.Contains<HiddenUnderFogInit>();
            var buildingInfo = init.Self.Info.TraitInfoOrDefault<BuildingInfo>();
            var footprintCells = buildingInfo != null ? buildingInfo.FrozenUnderFogTiles(init.Self.Location).ToList() : new List<CPos>() { init.Self.Location };
            footprint = footprintCells.SelectMany(c => map.ProjectedCellsCovering(c.ToMPos(map))).ToArray();

        }

        void INotifyCreated.Created(Actor self)
        {
            frozenStates = new PlayerDictionary<FrozenState>(self.World, (player, playerIndex) =>
            {
                var frozenActor = new FrozenActor(self, footprint, player, startsRevealed);
                if (startsRevealed)
                    UpdateFrozenActor(self, frozenActor, playerIndex);
                player.PlayerActor.Trait<FrozenActorLayer>().Add(frozenActor);

                return new FrozenState(frozenActor) { IsVisible = startsRevealed };
            });
        }

        void UpdateFrozenActor(Actor self,FrozenActor frozenActor,int playerIndex)
        {
            VisibilityHash |= 1 << (playerIndex % 32);
            frozenActor.RefreshState();
        }

        public bool IsVisible(Actor self,Player byPlayer)
        {
            return true;
        }

        void ITick.Tick(Actor self)
        {

        }

        void ITickRender.TickRender(EW.Graphics.WorldRenderer wr, Actor self)
        {

        }

        IEnumerable<IRenderable> IRenderModifier.ModifyRender(Actor self, WorldRenderer wr, IEnumerable<IRenderable> r)
        {
            return IsVisible(self, self.World.RenderPlayer) || isRendering ? r : SpriteRenderable.None;
        }

        IEnumerable<Rectangle> IRenderModifier.ModifyScreenBounds(Actor self, EW.Graphics.WorldRenderer wr, IEnumerable<Rectangle> bounds)
        {
            return bounds;
        }

    }

    public class HiddenUnderFogInit : IActorInit { }
}