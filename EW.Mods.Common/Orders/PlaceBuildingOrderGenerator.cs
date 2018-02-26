using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.Mods.Common.Graphics;
using EW.Mods.Common.Traits;
using EW.Primitives;
using EW.Traits;
using EW.NetWork;
using EW.Framework;
using EW.Framework.Touch;
namespace EW.Mods.Common.Orders
{
    public class PlaceBuildingOrderGenerator:IOrderGenerator
    {
        [Flags]
        enum CellType { Valid =0,Invalid = 1,LineBuild = 2}

        readonly ProductionQueue queue;
        readonly string building;
        readonly BuildingInfo buildingInfo;
        readonly PlaceBuildingInfo placeBuildingInfo;
        readonly BuildingInfluence buildingInfluence;
        readonly string faction;
        readonly Sprite buildOk;
        readonly Sprite buildBlocked;
        readonly GameViewPort viewport;
        readonly WVec centerOffset;
        readonly Int2 topLeftScreenOffset;

        IActorPreview[] preview;

        bool initialized;

        public PlaceBuildingOrderGenerator(ProductionQueue queue,string name,WorldRenderer worldRenderer)
        {
            var world = queue.Actor.World;
            this.queue = queue;
            viewport = worldRenderer.ViewPort;
            placeBuildingInfo = queue.Actor.Owner.PlayerActor.Info.TraitInfo<PlaceBuildingInfo>();
            building = name;


            var map = world.Map;
            var tileset = world.Map.Tileset.ToLowerInvariant();

            var info = map.Rules.Actors[building];
            buildingInfo = info.TraitInfo<BuildingInfo>();
            centerOffset = buildingInfo.CenterOffset(world);
            topLeftScreenOffset = -worldRenderer.ScreenPxOffset(centerOffset);

            var buildableInfo = info.TraitInfo<BuildableInfo>();
            var mostLikelyProducer = queue.MostLikelyProducer();

            faction = buildableInfo.ForceFaction ?? (mostLikelyProducer.Trait != null ? mostLikelyProducer.Trait.Faction : queue.Actor.Owner.Faction.InternalName);

            buildOk = map.Rules.Sequences.GetSequence("overlay", "build-valid-{0}".F(tileset)).GetSprite(0);
            buildBlocked = map.Rules.Sequences.GetSequence("overlay", "build-invalid").GetSprite(0);

            buildingInfluence = world.WorldActor.Trait<BuildingInfluence>();

        }


        public IEnumerable<Order> Order(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {
            var ret = InnerOrder(world, cell, gs).ToArray();

            //If there was a successful placement order
            if (ret.Any(o => o.OrderString == "PlaceBuilding" || o.OrderString == "LineBuild" || o.OrderString == "PlacePlug"))
                world.CancelInputMode();

            return ret;
        }

        IEnumerable<Order> InnerOrder(World world,CPos cell,GestureSample gs)
        {
            if (world.Paused)
                yield break;

            var owner = queue.Actor.Owner;

            if(gs.GestureType == GestureType.Tap)
            {
                var orderType = "PlaceBuilding";

                var topLeft = viewport.ViewToWorld(GameViewPort.LastMousePos + topLeftScreenOffset);

                var plugInfo = world.Map.Rules.Actors[building].TraitInfoOrDefault<PlugInfo>();

                if(plugInfo != null)
                {
                    orderType = "PlacePlug";

                    if (!AcceptsPlug(topLeft, plugInfo))
                    {
                        WarGame.Sound.PlayNotification(world.Map.Rules, owner, "Speech", "BuildingCannotPlaceAudio", owner.Faction.InternalName);
                        yield break;
                    }
                }
                else
                {
                    if(!world.CanPlaceBuilding(building,buildingInfo,topLeft,null) || !buildingInfo.IsCloseEnoughToBase(world, owner, building, topLeft))
                    {
                        foreach (var order in ClearBlockersOrders(world, topLeft))
                            yield return order;

                        WarGame.Sound.PlayNotification(world.Map.Rules, owner, "Speech", "BuildingCannotPlaceAudio", owner.Faction.InternalName);
                        yield break;

                    }

                    if (world.Map.Rules.Actors[building].HasTraitInfo<LineBuildInfo>())
                        orderType = "LineBuild";
                }

                yield return new Order(orderType, owner.PlayerActor, Target.FromCell(world, topLeft), false)
                {
                    //Building to place
                    TargetString = building,

                    //Actor to associate the placement with
                    ExtraData = queue.Actor.ActorID,

                    SuppressVisualFeedback = true
                };
            }
        }


        void IOrderGenerator.Tick(World world)
        {
            if (queue.CurrentItem() == null || queue.CurrentItem().Item != building)
                world.CancelInputMode();

            if (preview == null)
                return;

            foreach (var p in preview)
                p.Tick();
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr,World world) { yield break; }

        public IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr,World world)
        {
            var topLeft = viewport.ViewToWorld(GameViewPort.LastMousePos + topLeftScreenOffset);
            var centerPosition = world.Map.CenterOfCell(topLeft) + centerOffset;
            var rules = world.Map.Rules;

            var actorInfo = rules.Actors[building];
            foreach (var dec in actorInfo.TraitInfos<IPlaceBuildingDecorationInfo>())
                foreach (var r in dec.Render(wr, world, actorInfo, centerPosition))
                    yield return r;

            var cells = new Dictionary<CPos, CellType>();

            var plugInfo = rules.Actors[building].TraitInfoOrDefault<PlugInfo>();

            if(plugInfo != null)
            {
                if (buildingInfo.Dimensions.X != 1 || buildingInfo.Dimensions.Y != 1)
                    throw new InvalidOperationException("Plug requires a 1x1 sized Building");

                cells.Add(topLeft, MakeCellType(AcceptsPlug(topLeft, plugInfo)));

            }
            else if (rules.Actors[building].HasTraitInfo<LineBuildInfo>())
            {

            }
            else
            {
                if (!initialized)
                {
                    var actor = rules.Actors[building];

                    var td = new TypeDictionary()
                    {
                        new FactionInit(faction),
                        new OwnerInit(queue.Actor.Owner)
                    };

                    foreach (var api in actor.TraitInfos<IActorPreviewInitInfo>())
                        foreach (var o in api.ActorPreviewInits(actor, ActorPreviewType.PlaceBuilding))
                            td.Add(o);

                    var init = new ActorPreviewInitializer(actor, wr, td);
                    preview = actor.TraitInfos<IRenderActorPreviewInfo>()
                        .SelectMany(rpi => rpi.RenderPreview(init)).ToArray();
                    
                    initialized = true;
                }

                var previewRenderables = preview.SelectMany(p => p.Render(wr, centerPosition)).OrderBy(WorldRenderer.RenderableScreenZPositionComparisonKey);

                foreach (var r in previewRenderables)
                    yield return r;

                var res = world.WorldActor.TraitOrDefault<ResourceLayer>();
                var isCloseEnough = buildingInfo.IsCloseEnoughToBase(world, world.LocalPlayer, building, topLeft);
                foreach (var t in buildingInfo.Tiles(topLeft))
                    cells.Add(t, MakeCellType(isCloseEnough && world.IsCellBuildable(t, buildingInfo) && (res == null || res.GetResource(t) == null)));
            }

            var cellPalette = wr.Palette(placeBuildingInfo.Palette);
            var linePalette = wr.Palette(placeBuildingInfo.LineBuildSegmentPalette);
            var topLeftPos = world.Map.CenterOfCell(topLeft);

            foreach(var cell in cells)
            {
                var tile = !cell.Value.HasFlag(CellType.Invalid) ? buildOk : buildBlocked;
                var pal = cell.Value.HasFlag(CellType.LineBuild) ? linePalette : cellPalette;
                var pos = world.Map.CenterOfCell(cell.Key);

                yield return new SpriteRenderable(tile, pos, new WVec(0, 0, topLeftPos.Z - pos.Z), -511, pal, 1f, true);
            }
        }

        bool AcceptsPlug(CPos cell,PlugInfo plug)
        {
            var host = buildingInfluence.GetBuildingAt(cell);
            if (host == null)
                return false;

            var location = host.Location;
            return host.TraitsImplementing<Pluggable>().Any(p => location + p.Info.Offset == cell && p.AcceptsPlug(host, plug.Type));
        }

        CellType MakeCellType(bool valid,bool lineBuild = false)
        {
            var cell = valid ? CellType.Valid : CellType.Invalid;

            if (lineBuild)
                cell |= CellType.LineBuild;

            return cell;
        }

        public string GetCursor(World world,CPos cell,Int2 worldPixel,GestureSample gs) { return "default"; }


        IEnumerable<Order> ClearBlockersOrders(World world,CPos topLeft)
        {
            var allTiles = buildingInfo.Tiles(topLeft).ToArray();
            var neightborTiles = Util.ExpandFootprint(allTiles, true).Except(allTiles).Where(world.Map.Contains).ToList();

            var blockers = allTiles.SelectMany(world.ActorMap.GetActorsAt)
                .Where(a => a.Owner == queue.Actor.Owner && a.IsIdle)
                .Select(a => new TraitPair<Mobile>(a, a.TraitOrDefault<Mobile>()));

            foreach(var blocker in blockers.Where(x=>x.Trait != null))
            {
                var availableCells = neightborTiles.Where(t => blocker.Trait.CanEnterCell(t)).ToList();
                if (availableCells.Count == 0)
                    continue;

                yield return new Order("Move", blocker.Actor, Target.FromCell(world, blocker.Actor.ClosestCell(availableCells)), false)
                {
                    SuppressVisualFeedback = true
                };
            }
        }
    }
}