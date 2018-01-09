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

        public readonly HashSet<string> TargetTypes;

        public readonly Rectangle Bounds;

        readonly Actor actor;

        readonly Shroud shroud;

        public bool Visible = true;

        public bool Shrouded { get; private set; }

        public Player Owner { get; private set; }

        public bool IsValid { get { return Owner != null; } }

        public uint ID { get { return actor.ActorID; } }

        public ActorInfo Info { get { return actor.Info; } }

        public Actor Actor { get { return !actor.IsDead ? actor : null; } }

        public IRenderable[] Renderables = NoRenderables;
        static readonly IRenderable[] NoRenderables = new IRenderable[0];

        static readonly Rectangle[] NoBounds = new Rectangle[0];
        public Rectangle[] ScreenBounds = NoBounds;

        public Rectangle MouseBounds = Rectangle.Empty;
        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (Shrouded)
                return NoRenderables;

            return Renderables;
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

            partitionedFrozenActorIds = new SpatiallyPartitioned<uint>(world.Map.MapSize.X, world.Map.MapSize.Y, binSize);


            cols = world.Map.MapSize.X / binSize + 1;
            rows = world.Map.MapSize.Y / binSize + 1;

            dirtyBins = new bool[cols * rows];

        }


        public void Add(FrozenActor fa){

            frozenActorsById.Add(fa.ID,fa);
            world.ScreenMap.AddOrUpdate(owner,fa);


        }

        public void Remove(FrozenActor fa){

            partitionedFrozenActorIds.Remove(fa.ID);
            world.ScreenMap.Remove(owner,fa);
            frozenActorsById.Remove(fa.ID);
        }
        public void Tick(Actor self) 
        {
        
        
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

    }
}