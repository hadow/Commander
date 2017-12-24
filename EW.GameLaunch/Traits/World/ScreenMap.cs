using System;
using System.Collections.Generic;
using System.Linq;
using EW.Graphics;
using EW.OpenGLES;
using EW.Effects;
using EW.Primitives;
namespace EW.Traits
{

    public class ScreenMapInfo : ITraitInfo
    {
        /// <summary>
        /// Size of partition bins (world pixels)
        /// 分区大小
        /// </summary>
        public readonly int BinSize = 250;

        public object Create(ActorInitializer init) { return new ScreenMap(init.World,this); }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ScreenMap:IWorldLoaded
    {
        static readonly IEnumerable<FrozenActor> NoFrozenActors = new FrozenActor[0];

        readonly Func<Actor, bool> actorIsInWorld = a => a.IsInWorld;

        readonly Func<FrozenActor, bool> frozenActorIsValid = fa => fa.IsValid;

        
        WorldRenderer worldRenderer;

        readonly Cache<Player, SpatiallyPartitioned<FrozenActor>> partitionedFrozenActors;

        readonly SpatiallyPartitioned<Actor> partitionedActors;

        readonly SpatiallyPartitioned<IEffect> partitionedEffects;

        public ScreenMap(World world,ScreenMapInfo info)
        {
            var size = world.Map.Grid.TileSize;
            var width = world.Map.MapSize.X * size.Width;
            var height = world.Map.MapSize.Y * size.Height;

            partitionedFrozenActors = new Cache<Player, SpatiallyPartitioned<FrozenActor>>(_ => new SpatiallyPartitioned<FrozenActor>(width, height, info.BinSize));
            partitionedActors = new SpatiallyPartitioned<Actor>(width, height, info.BinSize);
            partitionedEffects = new SpatiallyPartitioned<IEffect>(width, height, info.BinSize);
        }
        public void WorldLoaded(World w,WorldRenderer wr)
        {
            worldRenderer = wr;
        }

        public IEnumerable<Actor> ActorsInBox(Int2 a,Int2 b)
        {
            return ActorsInBox(RectWithCorners(a, b));
        }

        Rectangle ActorBounds(Actor a)
        {
            var pos = worldRenderer.ScreenPxPosition(a.CenterPosition);
            var bounds = a.Bounds;
            bounds.Offset(pos.X, pos.Y);
            return bounds;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public IEnumerable<Actor> ActorsInBox(Rectangle r)
        {
            return partitionedActors.InBox(r).Where(actorIsInWorld);
        }
        static Rectangle RectWithCorners(Int2 a,Int2 b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public IEnumerable<FrozenActor> FrozenActorsInBox(Player p,Int2 a,Int2 b)
        {
            return FrozenActorsInBox(p, RectWithCorners(a, b));
        }

        public IEnumerable<FrozenActor> FrozenActorsInBox(Player p,Rectangle r)
        {
            if (p == null)
                return NoFrozenActors;
            return partitionedFrozenActors[p].InBox(r).Where(frozenActorIsValid);
        }


        //public IEnumerable<IEffect> EffectsInBox(Int2 a,Int2 b){
        //    return partitionee
        //}

        public void Add(Actor a)
        {
            partitionedActors.Add(a, ActorBounds(a));
        }

        public void Remove(Actor a)
        {
            partitionedActors.Remove(a);
        }


        public void Update(Actor a)
        {
            partitionedActors.Update(a, ActorBounds(a));
        }
    }
}