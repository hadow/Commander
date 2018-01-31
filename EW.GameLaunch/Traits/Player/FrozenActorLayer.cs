using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using EW.Graphics;
using EW.Primitives;
namespace EW.Traits
{
    public class FrozenActorLayerInfo : Requires<ShroudInfo>,ITraitInfo
    {
        /// <summary>
        /// Size of partition bins
        /// </summary>
        public readonly int BinSize = 10;
        public object Create(ActorInitializer init) { return new FrozenActorLayer(init.Self,this); }
    }

    public class FrozenActor
    {
        public readonly PPos[] Footprint;

        public readonly WPos CenterPosition;

        public HashSet<string> TargetTypes { get; private set; }

        public readonly Rectangle Bounds;

        readonly Actor actor;

        readonly Player viewer;

        readonly Shroud shroud;

        public bool Visible = true;

        public bool Shrouded { get; private set; }

        public Player Owner { get; private set; }

        public bool IsValid { get { return Owner != null; } }

        public uint ID { get { return actor.ActorID; } }

        public ActorInfo Info { get { return actor.Info; } }

        public Actor Actor { get { return !actor.IsDead ? actor : null; } }

        public Player Viewer{ get { return viewer; }}
        public IRenderable[] Renderables = NoRenderables;
        static readonly IRenderable[] NoRenderables = new IRenderable[0];

        static readonly Rectangle[] NoBounds = new Rectangle[0];
        public Rectangle[] ScreenBounds = NoBounds;

        public Rectangle MouseBounds = Rectangle.Empty;

        int flashTicks;

        public bool NeedRenderables { get; set; }

        public DamageState DamageState { get; private set; }

        readonly IHealth health;

        public int HP { get; private set; }

        public FrozenActor(Actor actor,PPos[] footprint,Player viewer,bool startsRevealed){

            this.actor = actor;
            this.viewer = viewer;

            shroud = viewer.Shroud;
            NeedRenderables = startsRevealed;

            //Consider all cells inside the map are(ignoring the current map bounds)
            Footprint = footprint.Where(m => shroud.Contains(m)).ToArray();

            if(Footprint.Length == 0)
            {
                throw new ArgumentException("The frozen actor has no footprint");
            }

            CenterPosition = actor.CenterPosition;
            TargetTypes = new HashSet<string>();

            health = actor.TraitOrDefault<IHealth>();

            UpdateVisibility();
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (Shrouded)
                return NoRenderables;

            if(flashTicks >0 && flashTicks % 2 == 0)
            {
                var highlight = wr.Palette("highlight");
                return Renderables.Concat(Renderables.Where(r => !r.IsDecoration).Select(r => r.WithPalette(highlight)));
            }

            return Renderables;
        }


        public void RefreshState()
        {
            Owner = actor.Owner;
            TargetTypes = actor.GetEnabledTargetTypes().ToHashSet();

            if(health != null)
            {
                HP = health.HP;
                DamageState = health.DamageState;
            }
        }

        public void UpdateVisibility()
        {
            var wasVisible = Visible;
            Shrouded = true;
            Visible = true;

            foreach(var puv in Footprint)
            {
                if(shroud.IsVisible(puv))
                {
                    Visible = false;
                    Shrouded = false;
                    break;
                }

                if (Shrouded && shroud.IsExplored(puv))
                    Shrouded = false;
            }
            NeedRenderables |= Visible && !wasVisible;
        }


        public void Flash(){
            flashTicks = 5;
        }

        public void Tick()
        {
            if (flashTicks > 0)
                flashTicks--;
        }
    }
    public class FrozenActorLayer:IRender,ITick,ISync
    {

        [Sync]
        public int VisibilityHash;

        [Sync]
        public int FrozenHash;

        readonly int binSize;

        readonly World world;

        readonly Player owner;

        readonly SpatiallyPartitioned<uint> partitionedFrozenActorIds;

        readonly Dictionary<uint, FrozenActor> frozenActorsById;

        readonly bool[] dirtyBins;

        readonly HashSet<uint> dirtyFrozenActorIds = new HashSet<uint>();

        readonly int rows, cols;



        public FrozenActorLayer(Actor self,FrozenActorLayerInfo info){

            binSize = info.BinSize;
            world = self.World;
            owner = self.Owner;

            frozenActorsById = new Dictionary<uint, FrozenActor>();

            //PERF:Partition the map into a series of coarse-grained bins and track changes in the shroud against bin
            //marking that bin dirty if it changes.This is fairly cheap to track and allows us to perform the expensive visibility update for frozen actors in these regions.
            partitionedFrozenActorIds = new SpatiallyPartitioned<uint>(world.Map.MapSize.X, world.Map.MapSize.Y, binSize);


            cols = world.Map.MapSize.X / binSize + 1;
            rows = world.Map.MapSize.Y / binSize + 1;

            dirtyBins = new bool[cols * rows];

            self.Trait<Shroud>().CellsChanged += cells =>
            {
                foreach (var cell in cells)
                {
                    var x = cell.U / binSize;
                    var y = cell.V / binSize;
                    dirtyBins[y * cols + x] = true;
                }
            };

        }


        public void Add(FrozenActor fa){

            frozenActorsById.Add(fa.ID,fa);
            world.ScreenMap.AddOrUpdate(owner,fa);
            partitionedFrozenActorIds.Add(fa.ID, FootprintBounds(fa));

        }

        public void Remove(FrozenActor fa){

            partitionedFrozenActorIds.Remove(fa.ID);
            world.ScreenMap.Remove(owner,fa);
            frozenActorsById.Remove(fa.ID);
        }

        void ITick.Tick(Actor self) 
        {
            UpdateDirtyFrozenActorsFromDirtyBins();

            var frozenActorsToRemove = new List<FrozenActor>();
            VisibilityHash = 0;
            FrozenHash = 0;
        

            foreach(var kvp in frozenActorsById)
            {
                var id = kvp.Key;
                var hash = (int)id;
                FrozenHash += hash;

                var frozenActor = kvp.Value;
                frozenActor.Tick();

                if (dirtyFrozenActorIds.Contains(id))
                    frozenActor.UpdateVisibility();

                if (frozenActor.Visible)
                    VisibilityHash += hash;
                else if (frozenActor.Actor == null)
                    frozenActorsToRemove.Add(frozenActor);
            }

            dirtyFrozenActorIds.Clear();

            foreach (var fa in frozenActorsToRemove)
                Remove(fa);
        }


        void UpdateDirtyFrozenActorsFromDirtyBins()
        {
            //Check which bins on the map were dirtied due to changes in the shroud and gather the frozen actors whose footprint overlap with these bins.
            for(var y = 0; y < rows; y++)
            {
                for(var x= 0; x < cols; x++)
                {
                    if (!dirtyBins[y * cols + x])
                        continue;

                    var box = new Rectangle(x * binSize, y * binSize, binSize, binSize);
                    dirtyFrozenActorIds.UnionWith(partitionedFrozenActorIds.InBox(box));
                }
            }

            Array.Clear(dirtyBins, 0, dirtyBins.Length);
        }


        public virtual IEnumerable<IRenderable> Render(Actor self,WorldRenderer wr)
        {
            //return world.ScreenMap.FrozenActorsInBox(owner, wr.ViewPort.TopLeft, wr.ViewPort.BottomRight).Where(f=>f.Visible).SelectMany(ff=>ff.Render(wr));
            return world.ScreenMap.RenderableFrozenActorsInBox(owner, wr.ViewPort.TopLeft, wr.ViewPort.BottomRight)
                        .Where(f=>f.Visible).SelectMany(ff=>ff.Render(wr));
        }


        public IEnumerable<Rectangle> ScreenBounds(Actor self,WorldRenderer wr)
        {
            //Player-actor render traits don't require screen bounds;
            yield break;
        }

        public FrozenActor FromID(uint id)
        {
            FrozenActor fa;
            if (!frozenActorsById.TryGetValue(id, out fa))
                return null;
            return fa;
        }

        public IEnumerable<FrozenActor> FrozenActorsInRegion(CellRegion region){

            var tl = region.TopLeft;
            var br = region.BottomRight;

            return partitionedFrozenActorIds.InBox(Rectangle.FromLTRB(tl.X, tl.Y, br.X, br.Y)).Select(FromID);
        }

        Rectangle FootprintBounds(FrozenActor fa)
        {
            var p1 = fa.Footprint[0];
            var minU = p1.U;
            var maxU = p1.U;
            var minV = p1.V;
            var maxV = p1.V;

            foreach(var p in fa.Footprint)
            {
                if (minU > p.U)
                    minU = p.U;
                else if (maxU < p.U)
                    maxU = p.U;

                if (minV > p.V)
                    minV = p.V;
                else if (maxV < p.V)
                    maxV = p.V;
            }

            return Rectangle.FromLTRB(minU, minV, maxU + 1, maxV + 1);
        }

    }
}