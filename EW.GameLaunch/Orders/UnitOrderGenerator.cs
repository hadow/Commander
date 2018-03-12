using System;
using System.Collections.Generic;
using System.Linq;
using EW.NetWork;
using EW.Framework;
using EW.Framework.Touch;
using EW.Traits;
using EW.Graphics;
namespace EW.Orders
{
    public class UnitOrderGenerator:IOrderGenerator
    {
        class UnitOrderResult
        {
            public readonly Actor Actor;
            public readonly IOrderTargeter Order;
            public readonly IIssueOrder Trait;
            public readonly string Cursor;
            public readonly Target Target;

            public UnitOrderResult(Actor actor,IOrderTargeter order,IIssueOrder trait,string cursor,Target target)
            {
                Actor = actor;
                Order = order;
                Trait = trait;
                Cursor = cursor;
                Target = target;
            }
                
        }
        
        public virtual IEnumerable<Order> Order(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {
            var target = TargetForInput(world, cell, worldPixel, gs);
            var actorsAt = world.ActorMap.GetActorsAt(cell).ToList();
            var orders = world.Selection.Actors.Select(a => OrderForUnit(a, target, actorsAt, cell, gs))
                .Where(o => o != null)
                .ToList();

            var actorsInvolved = orders.Select(o => o.Actor).Distinct();
            if (!actorsInvolved.Any())
                yield break;

            foreach (var o in orders)
                yield return CheckSameOrder(o.Order, o.Trait.IssueOrder(o.Actor, o.Order, o.Target, true));
        }

        static Order CheckSameOrder(IOrderTargeter iot,Order order)
        {
            if(order == null && iot.OrderID != null)
            {

            }
            else if(order !=null && iot.OrderID != order.OrderString)
            {

            }
            return order;
        }

        static Target TargetForInput(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {

            var actor = world.ScreenMap.ActorsAtMouse(gs)
                .Where(a => a.Actor.Info.HasTraitInfo<ITargetableInfo>() && !world.FogObscures(a.Actor))
                .WithHighestSelectionPriority(worldPixel);

            if (actor != null)
                return Target.FromActor(actor);

            var frozen = world.ScreenMap.FrozenActorsAtMouse(world.RenderPlayer, gs)
                .Where(a => a.Info.HasTraitInfo<ITargetableInfo>() && a.Visible && a.HasRenderables).WithHighestSelectionPriority(worldPixel);

            if (frozen != null)
                return Target.FromFrozenActor(frozen);

            return Target.FromCell(world, cell);
        }

        public virtual void Tick(World world) { }

        public virtual IEnumerable<IRenderable> Render(WorldRenderer wr,World world) { yield break; }

        public virtual IEnumerable<IRenderable> RenderAboveShroud(WorldRenderer wr,World world) { yield break; }

        public virtual string GetCursor(World world,CPos cell,Int2 worldPixel,GestureSample gs)
        {
            var useSelect = false;
            var target = TargetForInput(world, cell, worldPixel, gs);
            var actorsAt = world.ActorMap.GetActorsAt(cell).ToList();

            if (target.Type == TargetT.Actor && target.Actor.Info.HasTraitInfo<SelectableInfo>() && (!world.Selection.Actors.Any()))
                useSelect = true;

            var ordersWithCursor = world.Selection.Actors
                .Select(a => OrderForUnit(a, target, actorsAt, cell, gs))
                .Where(o => o != null && o.Cursor != null);

            var cursorOrder = ordersWithCursor.MaxByOrDefault(o => o.Order.OrderPriority);

            return cursorOrder != null ? cursorOrder.Cursor : (useSelect ? "select" : "default");
        }

        static UnitOrderResult OrderForUnit(Actor self,Target target,List<Actor> actorsAt,CPos xy,GestureSample gs)
        {
            if (self.Owner != self.World.LocalPlayer)
                return null;

            if (self.World.IsGameOver)
                return null;

            if (self.Disposed || !target.IsValidFor(self))
                return null;

            var orders = self.TraitsImplementing<IIssueOrder>()
                .SelectMany(trait => trait.Orders.Select(x => new { Trait = trait, Order = x }))
                .Select(x => x)
                .OrderByDescending(x => x.Order.OrderPriority);

            var modifiers = TargetModifiers.None;
            for(var i = 0; i < 2; i++)
            {
                foreach(var o in orders)
                {
                    var localModifiers = modifiers;
                    string cursor = null;
                    if (o.Order.CanTarget(self, target, actorsAt, ref localModifiers, ref cursor))
                        return new UnitOrderResult(self, o.Order, o.Trait, cursor, target);
                }

                //No valid orders,so check for orders against the cell
                target = Target.FromCell(self.World, xy);

            }

            return null;

        }
    }
}
