using System;
using System.Linq;
using System.Collections.Generic;
using EW.OpenGLES;
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
        public object Create(ActorInitializer init) { return new FrozenActorLayer(); }
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
        public IEnumerable<IRenderable> Render(WorldRenderer wr)
        {
            if (Shrouded)
                return NoRenderables;

            return Renderables;
        }
    }
    public class FrozenActorLayer:IRender,ITick,ISync
    {
        readonly int binSize;

        readonly World world;

        readonly Player owner;

        readonly SpatiallyPartitioned<uint> partitionedFrozenActorIds;

        readonly Dictionary<uint, FrozenActor> frozenActorsById;

        readonly bool[] dirtyBins;

        readonly HashSet<uint> dirtyFrozenActorIds = new HashSet<uint>();
        
        public void Tick(Actor self) { }

        public virtual IEnumerable<IRenderable> Render(Actor self,WorldRenderer wr)
        {
            return world.ScreenMap.FrozenActorsInBox(owner, wr.ViewPort.TopLeft, wr.ViewPort.BottomRight).Where(f=>f.Visible).SelectMany(ff=>ff.Render(wr));
        }

        public FrozenActor FromID(uint id)
        {
            FrozenActor fa;
            if (!frozenActorsById.TryGetValue(id, out fa))
                return null;
            return fa;
        }

    }
}