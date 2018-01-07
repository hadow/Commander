using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using EW.Graphics;
using EW.Framework;
using EW.Effects;
using EW.Primitives;
namespace EW.Traits
{

    public struct ActorBoundsPair:IEquatable<ActorBoundsPair>{

        public readonly Actor Actor;
        public readonly Rectangle Bounds;

        public ActorBoundsPair(Actor actor,Rectangle bounds){

            Actor = actor;
            Bounds = bounds;

            
        }

        public static bool operator ==(ActorBoundsPair me,ActorBoundsPair other){
            return me.Actor == other.Actor && Equals(me.Bounds, other.Bounds);
        }

        public static bool operator !=(ActorBoundsPair me,ActorBoundsPair other){
            return !(me == other);
        }

        public bool Equals(ActorBoundsPair other){
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorBoundsPair && Equals((ActorBoundsPair)obj);
        }

        public override int GetHashCode()
        {
            return Actor.GetHashCode() ^ Bounds.GetHashCode();

        }

        public override string ToString()
        {
            return "{0} -> {1}".F(Actor.Info.Name, Bounds.GetType().Name);
        }

    }


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

        readonly Func<Actor, ActorBoundsPair> selectActorAndBounds;

        readonly Cache<Player, SpatiallyPartitioned<FrozenActor>> partitionedMouseFrozenActors;

        readonly SpatiallyPartitioned<Actor> partitionedMouseActors;

        readonly Dictionary<Actor, ActorBoundsPair> partitionedMouseActorBounds = new Dictionary<Actor, ActorBoundsPair>();


        WorldRenderer worldRenderer;



        readonly Cache<Player, SpatiallyPartitioned<FrozenActor>> partitionedRenderableFrozenActors;

        readonly SpatiallyPartitioned<Actor> partitionedRenderableActors;

        readonly SpatiallyPartitioned<IEffect> partitionedRenderableEffects;



        //Update are done in one pass to ensure all bound changes have been applied.

        readonly HashSet<Actor> addOrUpdateActors = new HashSet<Actor>();
        readonly HashSet<Actor> removeActors = new HashSet<Actor>();

        readonly Cache<Player, HashSet<FrozenActor>> addOrUpdateFrozenActors;
        readonly Cache<Player, HashSet<FrozenActor>> removeFrozenActors;


        public ScreenMap(World world,ScreenMapInfo info)
        {
            var size = world.Map.Grid.TileSize;
            var width = world.Map.MapSize.X * size.Width;
            var height = world.Map.MapSize.Y * size.Height;

            partitionedRenderableFrozenActors = new Cache<Player, SpatiallyPartitioned<FrozenActor>>(_ => new SpatiallyPartitioned<FrozenActor>(width, height, info.BinSize));
            partitionedRenderableActors = new SpatiallyPartitioned<Actor>(width, height, info.BinSize);
            partitionedRenderableEffects = new SpatiallyPartitioned<IEffect>(width, height, info.BinSize);

            addOrUpdateFrozenActors = new Cache<Player, HashSet<FrozenActor>>(_ => new HashSet<FrozenActor>());
            removeFrozenActors = new Cache<Player, HashSet<FrozenActor>>(_ => new HashSet<FrozenActor>());

        }
        public void WorldLoaded(World w,WorldRenderer wr)
        {
            worldRenderer = wr;
        }


        public IEnumerable<IEffect> RenderableEffectsInBox(Int2 a,Int2 b){
            return partitionedRenderableEffects.InBox(RectWithCorners(a, b));

        }

        public IEnumerable<Actor> RenderableActorsInBox(Int2 a,Int2 b)
        {
            //return ActorsInBox(RectWithCorners(a, b));
            return partitionedRenderableActors.InBox(RectWithCorners(a, b)).Where(actorIsInWorld);

        }

        Rectangle ActorBounds(Actor a)
        {
            var pos = worldRenderer.ScreenPxPosition(a.CenterPosition);
            var bounds = a.Bounds;
            bounds.Offset(pos.X, pos.Y);
            return bounds;
        }

        public void AddOrUpdate(Player viewer,FrozenActor fa){

            if (removeFrozenActors[viewer].Contains(fa))
                removeFrozenActors[viewer].Remove(fa);

            addOrUpdateFrozenActors[viewer].Add(fa);

        }


        public void AddOrUpdate(Actor a){

            if (removeActors.Contains(a))
                removeActors.Remove(a);

            addOrUpdateActors.Add(a);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        //public IEnumerable<Actor> ActorsInBox(Rectangle r)
        //{
        //    return partitionedActors.InBox(r).Where(actorIsInWorld);
        //}


        static Rectangle RectWithCorners(Int2 a,Int2 b)
        {
            return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        //public IEnumerable<FrozenActor> FrozenActorsInBox(Player p,Int2 a,Int2 b)
        //{
        //    return FrozenActorsInBox(p, RectWithCorners(a, b));
        //}

        public IEnumerable<FrozenActor> RenderableFrozenActorsInBox(Player p,Int2 a,Int2 b)
        {
            if (p == null)
                return NoFrozenActors;
            return partitionedRenderableFrozenActors[p].InBox(RectWithCorners(a,b)).Where(frozenActorIsValid);
        }





        //public IEnumerable<IEffect> EffectsInBox(Int2 a,Int2 b){
        //    return partitionee
        //}

        //public void Add(Actor a)
        //{
        //    partitionedActors.Add(a, ActorBounds(a));
        //}

        //public void Remove(Actor a)
        //{
        //    partitionedActors.Remove(a);
        //}

        public void Remove(Actor a){

            removeActors.Add(a);
        }

        public void Remove(Player viewer,FrozenActor fa){

            removeFrozenActors[viewer].Add(fa);
        }

        public void Remove(IEffect effect){
            partitionedRenderableEffects.Remove(effect);
        }

        public void Add(IEffect effect,WPos position,Sprite sprite){

            var size = new Size((int)sprite.Size.X, (int)sprite.Size.Y);
            Add(effect,position,size);
        }

        public void Add(IEffect effect,WPos position,Size size){

            var screenPos = worldRenderer.ScreenPxPosition(position);

            var screenWidth = Math.Abs(size.Width);
            var screenHeight = Math.Abs(size.Height);

            var screenBounds = new Rectangle(screenPos.X - screenWidth / 2, screenPos.Y - screenHeight / 2, screenWidth, screenHeight);
            if (ValidBounds(screenBounds))
                partitionedRenderableEffects.Add(effect, screenBounds);
        }


        public void Update(IEffect effect,WPos position,Size size){

            Remove(effect);
            Add(effect,position,size);

        }
        //public void Update(Actor a)
        //{
        //    partitionedActors.Update(a, ActorBounds(a));
        //}


        static bool ValidBounds(Rectangle bounds){
            
            return bounds.Width > 0 && bounds.Height > 0;

        }

    }
}